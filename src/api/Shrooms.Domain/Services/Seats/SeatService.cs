using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Shrooms.Contracts.DAL;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.DataTransferObjects.Models.Seats;
using Shrooms.Contracts.Enums;
using Shrooms.DataLayer.EntityModels.Models;
using Shrooms.DataLayer.EntityModels.Models.Seats;

namespace Shrooms.Domain.Services.Seats
{
    public class SeatService : ISeatService
    {
        private const int HistoryWindowDays = 14;

        private readonly IUnitOfWork2 _uow;
        private readonly DbSet<Seat> _seatDbSet;
        private readonly DbSet<SeatReservation> _reservationDbSet;
        private readonly DbSet<SeatRelease> _releaseDbSet;
        private readonly DbSet<Room> _roomDbSet;
        private readonly DbSet<ApplicationUser> _userDbSet;

        public SeatService(IUnitOfWork2 uow)
        {
            _uow = uow;
            _seatDbSet = uow.GetDbSet<Seat>();
            _reservationDbSet = uow.GetDbSet<SeatReservation>();
            _releaseDbSet = uow.GetDbSet<SeatRelease>();
            _roomDbSet = uow.GetDbSet<Room>();
            _userDbSet = uow.GetDbSet<ApplicationUser>();
        }

        public async Task<SeatBoardDto> GetBoardAsync(SeatBoardArgsDto args)
        {
            var from = SeatWireFormat.ParseDay(args.From) ?? DateTime.UtcNow.Date;
            var to = SeatWireFormat.ParseDay(args.To) ?? from.AddDays(13);
            if (to < from)
            {
                to = from;
            }

            var historyFrom = from.AddDays(-HistoryWindowDays);

            var floorSeats = await _seatDbSet
                .AsNoTracking()
                .Include(seat => seat.Owner)
                .Include(seat => seat.Room)
                .Where(seat => seat.OrganizationId == args.OrganizationId && seat.Room.FloorId == args.FloorId)
                .ToListAsync();

            var floorSeatIds = floorSeats.Select(seat => seat.Id).ToList();

            var reservations = await _reservationDbSet
                .AsNoTracking()
                .Include(reservation => reservation.ApplicationUser)
                .Where(reservation => reservation.OrganizationId == args.OrganizationId
                                      && floorSeatIds.Contains(reservation.SeatId)
                                      && reservation.Day >= from
                                      && reservation.Day <= to)
                .ToListAsync();

            var releases = await _releaseDbSet
                .AsNoTracking()
                .Where(release => release.OrganizationId == args.OrganizationId
                                  && floorSeatIds.Contains(release.SeatId)
                                  && release.Day >= from
                                  && release.Day <= to)
                .ToListAsync();

            var myReservations = await _reservationDbSet
                .AsNoTracking()
                .Include(reservation => reservation.Seat).ThenInclude(seat => seat.Room)
                .Include(reservation => reservation.Seat).ThenInclude(seat => seat.Owner)
                .Where(reservation => reservation.OrganizationId == args.OrganizationId
                                      && reservation.ApplicationUserId == args.UserId
                                      && reservation.Day >= historyFrom
                                      && reservation.Day <= to)
                .ToListAsync();

            var myOwnedSeats = await _seatDbSet
                .AsNoTracking()
                .Include(seat => seat.Owner)
                .Include(seat => seat.Room)
                .Where(seat => seat.OrganizationId == args.OrganizationId && seat.OwnerId == args.UserId)
                .ToListAsync();

            var mySeatIds = myOwnedSeats.Select(seat => seat.Id)
                .Concat(myReservations.Select(reservation => reservation.SeatId))
                .Distinct()
                .ToList();

            var myReleases = await _releaseDbSet
                .AsNoTracking()
                .Where(release => release.OrganizationId == args.OrganizationId
                                  && mySeatIds.Contains(release.SeatId)
                                  && release.Day >= from
                                  && release.Day <= to)
                .ToListAsync();

            var mySeats = myOwnedSeats
                .Concat(myReservations.Select(reservation => reservation.Seat))
                .Where(seat => seat != null)
                .GroupBy(seat => seat.Id)
                .Select(group => group.First())
                .ToList();

            return new SeatBoardDto
            {
                FloorId = args.FloorId,
                From = SeatWireFormat.ToDay(from),
                To = SeatWireFormat.ToDay(to),
                Seats = floorSeats.Select(ToDto).ToList(),
                Reservations = reservations.Select(ToDto).ToList(),
                Releases = releases.Select(ToDto).ToList(),
                MySeats = mySeats.Select(ToDto).ToList(),
                MyReservations = myReservations
                    .Where(reservation => reservation.Day >= from)
                    .Select(ToDto)
                    .ToList(),
                MyReleases = myReleases.Select(ToDto).ToList(),
                MyHistory = myReservations
                    .Where(reservation => reservation.Day < from)
                    .Select(reservation => new SeatHistoryDto
                    {
                        SeatId = reservation.SeatId,
                        Day = SeatWireFormat.ToDay(reservation.Day)
                    })
                    .ToList()
            };
        }

        public async Task<IEnumerable<SeatDto>> GetByRoomAsync(int roomId, UserAndOrganizationDto userOrg)
        {
            var seats = await _seatDbSet
                .AsNoTracking()
                .Include(seat => seat.Owner)
                .Include(seat => seat.Room)
                .Where(seat => seat.OrganizationId == userOrg.OrganizationId && seat.RoomId == roomId)
                .OrderBy(seat => seat.Name)
                .ToListAsync();

            return seats.Select(ToDto).ToList();
        }

        public async Task<SeatBookResultDto> BookAsync(SeatDayArgsDto args)
        {
            var day = SeatWireFormat.ParseDay(args.Day);
            if (day == null)
            {
                throw new ArgumentException("A booking needs a calendar day.", nameof(args));
            }

            var seat = await _seatDbSet
                .Include(candidate => candidate.Room)
                .FirstOrDefaultAsync(candidate => candidate.Id == args.SeatId
                                                  && candidate.OrganizationId == args.OrganizationId);

            if (seat == null)
            {
                throw new InvalidOperationException("Desk not found.");
            }

            var takenByOther = await _reservationDbSet
                .AnyAsync(reservation => reservation.SeatId == seat.Id
                                         && reservation.Day == day
                                         && reservation.ApplicationUserId != args.UserId);

            if (takenByOther)
            {
                throw new InvalidOperationException("Desk already taken for that day.");
            }

            var released = await _releaseDbSet
                .FirstOrDefaultAsync(release => release.SeatId == seat.Id && release.Day == day);

            var ownedBySomeoneElse = seat.Type == SeatType.Permanent
                                     && seat.OwnerId != null
                                     && seat.OwnerId != args.UserId
                                     && released == null;

            if (ownedBySomeoneElse)
            {
                throw new InvalidOperationException("Desk belongs to someone else.");
            }

            var mine = await FindMyDeskAsync(args.UserId, args.OrganizationId, day.Value);
            var moving = mine != null && mine.Seat.Id != seat.Id;
            var movedFrom = moving ? ToDto(mine.Seat) : null;

            if (moving)
            {
                VacateOwnedSeat(mine, day.Value);
            }

            var ownDesk = seat.Type == SeatType.Permanent && seat.OwnerId == args.UserId;

            if (ownDesk)
            {
                if (released != null)
                {
                    _releaseDbSet.Remove(released);
                }

                if (moving && mine.Reservation != null)
                {
                    _reservationDbSet.Remove(mine.Reservation);
                }
            }
            else if (moving && mine.Reservation != null)
            {
                mine.Reservation.SeatId = seat.Id;
            }
            else
            {
                var alreadyMine = await _reservationDbSet
                    .AnyAsync(reservation => reservation.SeatId == seat.Id
                                             && reservation.Day == day
                                             && reservation.ApplicationUserId == args.UserId);

                if (!alreadyMine)
                {
                    _reservationDbSet.Add(new SeatReservation
                    {
                        SeatId = seat.Id,
                        Day = day.Value,
                        ApplicationUserId = args.UserId,
                        OrganizationId = args.OrganizationId
                    });
                }
            }

            await SaveOrTranslateAsync(args.UserId);

            return new SeatBookResultDto
            {
                SeatId = seat.Id,
                Day = SeatWireFormat.ToDay(day.Value),
                MovedFrom = movedFrom
            };
        }

        public async Task GoHomeAsync(string day, UserAndOrganizationDto userOrg)
        {
            var parsed = SeatWireFormat.ParseDay(day);
            if (parsed == null)
            {
                throw new ArgumentException("A day is required.", nameof(day));
            }

            var mine = await FindMyDeskAsync(userOrg.UserId, userOrg.OrganizationId, parsed.Value);
            if (mine == null)
            {
                return;
            }

            await ReleaseMyDeskAsync(mine, parsed.Value, userOrg.OrganizationId);
            await SaveOrTranslateAsync(userOrg.UserId);
        }

        public async Task UnreleaseAsync(SeatDayArgsDto args)
        {
            var day = SeatWireFormat.ParseDay(args.Day);
            if (day == null)
            {
                throw new ArgumentException("A day is required.", nameof(args));
            }

            var seat = await _seatDbSet
                .FirstOrDefaultAsync(candidate => candidate.Id == args.SeatId
                                                  && candidate.OrganizationId == args.OrganizationId
                                                  && candidate.OwnerId == args.UserId);

            if (seat == null)
            {
                throw new InvalidOperationException("Desk is not yours.");
            }

            var claimed = await _reservationDbSet
                .AnyAsync(reservation => reservation.SeatId == seat.Id && reservation.Day == day);

            if (claimed)
            {
                throw new InvalidOperationException("Desk already taken for that day.");
            }

            var release = await _releaseDbSet
                .FirstOrDefaultAsync(candidate => candidate.SeatId == seat.Id && candidate.Day == day);

            if (release == null)
            {
                return;
            }

            var elsewhere = await FindMyDeskAsync(args.UserId, args.OrganizationId, day.Value);
            if (elsewhere != null && elsewhere.Seat.Id != seat.Id)
            {
                await ReleaseMyDeskAsync(elsewhere, day.Value, args.OrganizationId);
            }

            _releaseDbSet.Remove(release);
            await SaveOrTranslateAsync(args.UserId);
        }

        public async Task<SeatDto> CreateAsync(SeatSaveArgsDto args)
        {
            var room = await _roomDbSet
                .AsNoTracking()
                .FirstOrDefaultAsync(candidate => candidate.Id == args.RoomId
                                                  && candidate.OrganizationId == args.OrganizationId);

            if (room == null)
            {
                throw new InvalidOperationException("Room not found.");
            }

            var name = ValidatedName(args.Name);
            var type = ValidatedType(args.Type);
            var ownerId = type == SeatType.Permanent ? args.OwnerId : null;
            await EnsureOwnerAsync(ownerId, args.OrganizationId);

            var seat = new Seat
            {
                Name = name,
                Type = type,
                X = args.X,
                Y = args.Y,
                RoomId = args.RoomId,
                OwnerId = ownerId,
                OrganizationId = args.OrganizationId
            };

            _seatDbSet.Add(seat);
            await _uow.SaveChangesAsync(args.UserId);

            if (await VacateOtherDesksAsync(seat))
            {
                await _uow.SaveChangesAsync(args.UserId);
            }

            await FreeDaysHeldElsewhereAsync(seat, args.UserId);

            return await LoadDtoAsync(seat.Id, args.OrganizationId);
        }

        public async Task<SeatDto> UpdateAsync(SeatSaveArgsDto args)
        {
            var seat = await _seatDbSet
                .FirstOrDefaultAsync(candidate => candidate.Id == args.Id
                                                  && candidate.OrganizationId == args.OrganizationId);

            if (seat == null)
            {
                throw new InvalidOperationException("Desk not found.");
            }

            var name = ValidatedName(args.Name);
            var type = ValidatedType(args.Type);
            var ownerId = type == SeatType.Permanent ? args.OwnerId : null;
            await EnsureOwnerAsync(ownerId, args.OrganizationId);

            seat.Name = name;
            seat.Type = type;
            seat.X = args.X;
            seat.Y = args.Y;
            seat.OwnerId = ownerId;

            await VacateOtherDesksAsync(seat);
            await _uow.SaveChangesAsync(args.UserId);
            await FreeDaysHeldElsewhereAsync(seat, args.UserId);

            return await LoadDtoAsync(seat.Id, args.OrganizationId);
        }

        public async Task DeleteAsync(int id, UserAndOrganizationDto userOrg)
        {
            var seat = await _seatDbSet
                .FirstOrDefaultAsync(candidate => candidate.Id == id
                                                  && candidate.OrganizationId == userOrg.OrganizationId);

            if (seat == null)
            {
                throw new InvalidOperationException("Desk not found.");
            }

            var reservations = await _reservationDbSet.Where(r => r.SeatId == id).ToListAsync();
            var releases = await _releaseDbSet.Where(r => r.SeatId == id).ToListAsync();

            _reservationDbSet.RemoveRange(reservations);
            _releaseDbSet.RemoveRange(releases);
            seat.IsDeleted = true;

            await _uow.SaveChangesAsync(userOrg.UserId);
        }

        private sealed class MyDesk
        {
            public Seat Seat { get; init; }

            public SeatReservation Reservation { get; init; }
        }

        private async Task<MyDesk> FindMyDeskAsync(string userId, int organizationId, DateTime day)
        {
            var reservation = await _reservationDbSet
                .Include(candidate => candidate.Seat).ThenInclude(seat => seat.Room)
                .FirstOrDefaultAsync(candidate => candidate.OrganizationId == organizationId
                                                  && candidate.ApplicationUserId == userId
                                                  && candidate.Day == day);

            if (reservation != null)
            {
                return new MyDesk { Seat = reservation.Seat, Reservation = reservation };
            }

            var owned = await _seatDbSet
                .Include(seat => seat.Room)
                .Where(seat => seat.OrganizationId == organizationId
                               && seat.OwnerId == userId
                               && seat.Type == SeatType.Permanent)
                .ToListAsync();

            foreach (var seat in owned)
            {
                var releasedThatDay = await _releaseDbSet
                    .AnyAsync(release => release.SeatId == seat.Id && release.Day == day);

                if (!releasedThatDay)
                {
                    return new MyDesk { Seat = seat };
                }
            }

            return null;
        }

        private async Task ReleaseMyDeskAsync(MyDesk mine, DateTime day, int organizationId)
        {
            if (mine.Reservation != null)
            {
                _reservationDbSet.Remove(mine.Reservation);
            }

            if (!NeedsRelease(mine))
            {
                return;
            }

            var alreadyReleased = await _releaseDbSet
                .AnyAsync(release => release.SeatId == mine.Seat.Id && release.Day == day);

            if (!alreadyReleased)
            {
                _releaseDbSet.Add(new SeatRelease
                {
                    SeatId = mine.Seat.Id,
                    Day = day,
                    OrganizationId = organizationId
                });
            }
        }

        private static bool NeedsRelease(MyDesk mine)
        {
            return mine.Seat.Type == SeatType.Permanent
                   && mine.Seat.OwnerId != null
                   && (mine.Reservation == null
                       || mine.Seat.OwnerId == mine.Reservation.ApplicationUserId);
        }

        private void VacateOwnedSeat(MyDesk mine, DateTime day)
        {
            if (!NeedsRelease(mine)
                || _releaseDbSet.Local.Any(release => release.SeatId == mine.Seat.Id && release.Day == day))
            {
                return;
            }

            _releaseDbSet.Add(new SeatRelease
            {
                SeatId = mine.Seat.Id,
                Day = day,
                OrganizationId = mine.Seat.OrganizationId
            });
        }

        private async Task SaveOrTranslateAsync(string userId)
        {
            try
            {
                await _uow.SaveChangesAsync(userId);
            }
            catch (DbUpdateException e) when (IsUniqueViolation(e))
            {
                throw new InvalidOperationException("Desk already taken for that day.");
            }
        }

        private static bool IsUniqueViolation(DbUpdateException e)
        {
            return e.InnerException is SqlException sql
                   && (sql.Number == 2601 || sql.Number == 2627);
        }

        private static string ValidatedName(string value)
        {
            var name = value?.Trim();

            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException("A desk needs a name.", nameof(value));
            }

            if (name.Length > Seat.MaxNameLength)
            {
                throw new ArgumentException(
                    $"A desk name cannot be longer than {Seat.MaxNameLength} characters.",
                    nameof(value));
            }

            return name;
        }

        private static SeatType ValidatedType(string value)
        {
            return SeatWireFormat.TryParseType(value, out var type)
                ? type
                : throw new ArgumentException($"Unknown desk type '{value}'.", nameof(value));
        }

        private async Task EnsureOwnerAsync(string ownerId, int organizationId)
        {
            if (string.IsNullOrEmpty(ownerId))
            {
                return;
            }

            var belongs = await _userDbSet
                .AsNoTracking()
                .AnyAsync(user => user.Id == ownerId && user.OrganizationId == organizationId);

            if (!belongs)
            {
                throw new InvalidOperationException("Employee not found.");
            }
        }

        // One permanent desk per person. Enforced here rather than in the client,
        // which can only pretend: it has no way to write the other rows.
        private async Task<bool> VacateOtherDesksAsync(Seat seat)
        {
            if (seat.Type != SeatType.Permanent || string.IsNullOrEmpty(seat.OwnerId))
            {
                return false;
            }

            var others = await _seatDbSet
                .Where(candidate => candidate.OrganizationId == seat.OrganizationId
                                    && candidate.OwnerId == seat.OwnerId
                                    && candidate.Id != seat.Id)
                .ToListAsync();

            if (others.Count == 0)
            {
                return false;
            }

            var vacatedIds = others.Select(other => other.Id).ToList();
            foreach (var other in others)
            {
                other.OwnerId = null;
            }

            // A release records that the desk's owner freed it. With no owner it
            // says nothing, and would render as a stray "freed up" desk.
            var strandedReleases = await _releaseDbSet
                .Where(release => vacatedIds.Contains(release.SeatId))
                .ToListAsync();
            _releaseDbSet.RemoveRange(strandedReleases);

            return true;
        }

        private async Task FreeDaysHeldElsewhereAsync(Seat seat, string actingUserId)
        {
            if (seat.Type != SeatType.Permanent || string.IsNullOrEmpty(seat.OwnerId))
            {
                return;
            }

            var today = DateTime.UtcNow.Date;
            var clashes = await _reservationDbSet
                .AsNoTracking()
                .Where(reservation => reservation.OrganizationId == seat.OrganizationId
                                      && reservation.ApplicationUserId == seat.OwnerId
                                      && reservation.SeatId != seat.Id
                                      && reservation.Day >= today)
                .Select(reservation => reservation.Day)
                .ToListAsync();

            if (clashes.Count == 0)
            {
                return;
            }

            var released = await _releaseDbSet
                .AsNoTracking()
                .Where(release => release.SeatId == seat.Id && release.Day >= today)
                .Select(release => release.Day)
                .ToListAsync();

            foreach (var day in clashes.Distinct().Except(released))
            {
                _releaseDbSet.Add(new SeatRelease
                {
                    SeatId = seat.Id,
                    Day = day,
                    OrganizationId = seat.OrganizationId
                });
            }

            await _uow.SaveChangesAsync(actingUserId);
        }

        private async Task<SeatDto> LoadDtoAsync(int id, int organizationId)
        {
            var seat = await _seatDbSet
                .AsNoTracking()
                .Include(candidate => candidate.Owner)
                .Include(candidate => candidate.Room)
                .FirstOrDefaultAsync(candidate => candidate.Id == id && candidate.OrganizationId == organizationId);

            return seat == null ? null : ToDto(seat);
        }

        private static SeatDto ToDto(Seat seat)
        {
            return new SeatDto
            {
                Id = seat.Id,
                Name = seat.Name,
                Type = SeatWireFormat.ToWire(seat.Type),
                X = seat.X,
                Y = seat.Y,
                RoomId = seat.RoomId,
                FloorId = seat.Room?.FloorId,
                Owner = ToDto(seat.Owner)
            };
        }

        private static SeatReservationDto ToDto(SeatReservation reservation)
        {
            return new SeatReservationDto
            {
                SeatId = reservation.SeatId,
                Day = SeatWireFormat.ToDay(reservation.Day),
                User = ToDto(reservation.ApplicationUser)
            };
        }

        private static SeatReleaseDto ToDto(SeatRelease release)
        {
            return new SeatReleaseDto
            {
                SeatId = release.SeatId,
                Day = SeatWireFormat.ToDay(release.Day)
            };
        }

        private static SeatPersonDto ToDto(ApplicationUser user)
        {
            return user == null
                ? null
                : new SeatPersonDto
                {
                    Id = user.Id,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    PictureId = user.PictureId
                };
        }
    }
}
