INSERT INTO dbo.AspNetUserRoles
SELECT u.Id as UserId, r.Id as RoleId 
FROM dbo.AspNetUsers u
JOIN dbo.AspNetRoles r ON r.Name = 'Admin'

INSERT INTO dbo.WallMembers
SELECT 
    1 as WallId,
    u.Id as UserId,
    GETDATE() as Created,
	NULL as CreatedBy,
    GETDATE() as Modified,
	NULL as ModifiedBy,
    0 as IsDeleted,
    0 as EmailNotificationsEnabled,
    1 as AppNotificationsEnabled 
FROM dbo.AspNetUsers u
JOIN dbo.AspNetRoles r ON r.Name = 'Admin'