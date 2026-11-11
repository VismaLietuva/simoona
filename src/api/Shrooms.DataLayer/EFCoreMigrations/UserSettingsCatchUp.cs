namespace Shrooms.DataLayer.EFCoreMigrations
{
    /// <summary>
    /// Brings UserSettings up to date with what pre-UserSettings code wrote to the old
    /// AspNetUsers columns after <see cref="AddUserSettings"/> ran. During a deploy the
    /// migration runs when the new instance starts, while old instances may still be
    /// serving requests; they keep writing the old columns and create users without a
    /// settings row.
    /// <para>
    /// Run it from the migration that drops those columns, right before dropping them.
    /// That migration must ship in a later deploy than AddUserSettings, so every old
    /// instance is gone by the time this runs. Idempotent.
    /// </para>
    /// </summary>
    internal static class UserSettingsCatchUp
    {
        public const string Sql = @"
-- Every AddUserSettings backfill row was written by one statement, so it has the earliest Created.
DECLARE @backfill datetime2 = (SELECT MIN([Created]) FROM dbo.UserSettings);

-- 1. Users created by an old instance have no row: copy theirs from the old columns.
INSERT dbo.UserSettings ([UserId], [CultureCode], [TimeZone], [DailyMailingHour], [IsTutorialComplete], [Preferences], [Created], [CreatedBy], [Modified], [ModifiedBy])
SELECT u.[Id], u.[CultureCode], u.[TimeZone], u.[DailyMailingHour], u.[IsTutorialComplete],
       CASE WHEN u.[IsAnonymized] = 1 THEN NULL ELSE u.[WallWidgetPreferences] END,
       SYSUTCDATETIME(), NULL, SYSUTCDATETIME(), NULL
FROM dbo.AspNetUsers u
WHERE NOT EXISTS (SELECT 1 FROM dbo.UserSettings s WHERE s.[UserId] = u.[Id]);

IF @backfill IS NULL RETURN;

-- 2. Rows the new code created on demand (GetOrCreateSettings) start without a culture;
--    user creation always sets one. Fill what is missing from the old columns.
UPDATE s
SET s.[CultureCode] = COALESCE(s.[CultureCode], u.[CultureCode]),
    s.[TimeZone] = COALESCE(s.[TimeZone], u.[TimeZone]),
    s.[DailyMailingHour] = COALESCE(s.[DailyMailingHour], u.[DailyMailingHour])
FROM dbo.UserSettings s
JOIN dbo.AspNetUsers u ON u.[Id] = s.[UserId]
WHERE s.[Created] > @backfill AND s.[CultureCode] IS NULL;

-- 3. Only old code writes the old columns, so where they differ from a backfill row the
--    new code never touched (Modified still equals Created), an old instance changed
--    them: copy the change. A row the new code changed keeps the new code's values.
UPDATE s
SET s.[CultureCode] = u.[CultureCode],
    s.[TimeZone] = u.[TimeZone],
    s.[DailyMailingHour] = u.[DailyMailingHour],
    s.[IsTutorialComplete] = u.[IsTutorialComplete],
    s.[Preferences] = CASE WHEN u.[IsAnonymized] = 1 THEN NULL ELSE u.[WallWidgetPreferences] END,
    s.[Modified] = SYSUTCDATETIME()
FROM dbo.UserSettings s
JOIN dbo.AspNetUsers u ON u.[Id] = s.[UserId]
WHERE s.[Created] = @backfill AND s.[Modified] = s.[Created]
  AND EXISTS (SELECT s.[CultureCode], s.[TimeZone], s.[DailyMailingHour], s.[IsTutorialComplete], s.[Preferences]
              EXCEPT
              SELECT u.[CultureCode], u.[TimeZone], u.[DailyMailingHour], u.[IsTutorialComplete],
                     CASE WHEN u.[IsAnonymized] = 1 THEN NULL ELSE u.[WallWidgetPreferences] END);
";
    }
}
