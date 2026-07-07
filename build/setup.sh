#!/usr/bin/env bash
# =============================================================================
# Simoona – DB setup script (Linux / macOS)
#
# Seeds reference data and creates the first admin user.
# Connects directly to SQL Server via sqlcmd (must be installed on the host).
# EF Core migrations must be applied before running this script.
#
# Usage:
#   ./setup.sh <connection-string> <email> <password> [org-name]
#
# Example:
#   ./setup.sh "Server=localhost,1434;Database=SimoonaDB;User Id=sa;Password=Password!123;TrustServerCertificate=True" \
#              admin@company.com 'S3cur3P@ss!'
#   ./setup.sh "..." admin@company.com 'S3cur3P@ss!' acme
# =============================================================================
set -euo pipefail

CONN_STR="${1:-}"
EMAIL="${2:-}"
PASSWORD="${3:-}"
ORG_NAME="${4:-}"

if [[ -z "$CONN_STR" || -z "$EMAIL" || -z "$PASSWORD" ]]; then
    echo "Usage: $0 <connection-string> <email> <password> [org-name]" >&2
    exit 1
fi

# Parse ADO.NET connection string into sqlcmd args using Python
read -r SERVER DATABASE DB_USER DB_PASS <<< "$(python3 - "$CONN_STR" <<'PYEOF'
import sys, re
cs = sys.argv[1]
def get(keys):
    for k in keys:
        m = re.search(rf'(?i){re.escape(k)}\s*=\s*([^;]+)', cs)
        if m: return m.group(1).strip()
    return ''
print(get(['Server','Data Source']), get(['Database','Initial Catalog']), get(['User Id','UID']), get(['Password','PWD']))
PYEOF
)"

SQL_ARGS=(-S "$SERVER" -d "$DATABASE" -b -No)
if [[ -n "$DB_USER" ]]; then SQL_ARGS+=(-U "$DB_USER" -P "$DB_PASS"); fi  # else Windows auth

run_sql() {
    sqlcmd "${SQL_ARGS[@]}" -Q "$1"
}

# -- 1. Seed reference data ---------------------------------------------------
echo "-> Seeding reference data..."
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
sqlcmd "${SQL_ARGS[@]}" -i "$SCRIPT_DIR/seed.sql"
echo "   Reference data seeded."

# -- 2. Update organisation name (optional) -----------------------------------
if [[ -n "$ORG_NAME" ]]; then
    echo "-> Updating organisation name to '$ORG_NAME'..."
    run_sql "UPDATE dbo.Organizations SET Name='$ORG_NAME', ShortName='$ORG_NAME' WHERE Id=1"
    echo "   Organisation updated."
fi

# -- 3. Compute ASP.NET Core Identity V3 password hash (PBKDF2-SHA256) --------
echo "-> Hashing password..."
PASSWORD_HASH=$(python3 - "$PASSWORD" <<'PYEOF'
import sys, hashlib, os, struct, base64
password = sys.argv[1]
salt = os.urandom(16)
subkey = hashlib.pbkdf2_hmac('sha256', password.encode('utf-8'), salt, 10000, dklen=32)
header = bytes([0x01]) + struct.pack('>I', 1) + struct.pack('>I', 10000) + struct.pack('>I', 16)
print(base64.b64encode(header + salt + subkey).decode('ascii'))
PYEOF
)

SECURITY_STAMP=$(python3 -c "import os,base64; print(base64.b64encode(os.urandom(20)).decode())")
USER_ID=$(python3 -c "import uuid; print(str(uuid.uuid4()))")
NORMALISED=$(echo "$EMAIL" | tr '[:lower:]' '[:upper:]')

# -- 4. Check if user already exists ------------------------------------------
EXISTING=$(sqlcmd "${SQL_ARGS[@]}" -h -1 \
    -Q "SET NOCOUNT ON; SELECT COUNT(1) FROM dbo.AspNetUsers WHERE NormalizedEmail='$NORMALISED'")
if [[ "${EXISTING//[[:space:]]/}" != "0" ]]; then
    echo "   User '$EMAIL' already exists - skipping."
    exit 0
fi

# -- 5. Insert admin user ------------------------------------------------------
echo "-> Creating admin user '$EMAIL'..."
run_sql "INSERT INTO dbo.AspNetUsers \
    (Id,FirstName,LastName,OrganizationId,Email,NormalizedEmail, \
     EmailConfirmed,PhoneNumberConfirmed,TwoFactorEnabled,LockoutEnabled, \
     AccessFailedCount,UserName,NormalizedUserName,SecurityStamp, \
     IsManagingDirector,EmploymentDate,PasswordHash,IsDeleted,ConcurrencyStamp) \
VALUES \
    ('$USER_ID','Admin','Admin',1,'$EMAIL','$NORMALISED', \
     1,0,0,0, \
     0,'$EMAIL','$NORMALISED','$SECURITY_STAMP', \
     1,GETDATE(),'$PASSWORD_HASH',0,NEWID())"

run_sql "INSERT INTO dbo.AspNetUserRoles (UserId,RoleId) \
SELECT '$USER_ID',Id FROM dbo.AspNetRoles WHERE NormalizedName='ADMIN'"

run_sql "INSERT INTO dbo.WallModerators (WallId,UserId,IsDeleted,Created,Modified) \
VALUES (1,'$USER_ID',0,'1900-01-01','1900-01-01')"

run_sql "INSERT INTO dbo.WallMembers \
    (WallId,UserId,Created,CreatedBy,Modified,ModifiedBy,IsDeleted,EmailNotificationsEnabled,AppNotificationsEnabled) \
VALUES (1,'$USER_ID',GETDATE(),NULL,GETDATE(),NULL,0,0,1)"

echo "   Admin user '$EMAIL' created."
echo "Done."