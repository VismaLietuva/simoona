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
    /// instance is gone by the time this runs. AddUserSettings.Down runs it too, so a
    /// rollback keeps what old instances wrote. Idempotent.
    /// </para>
    /// <para>
    /// A backfill row is one AddUserSettings wrote: it carries <see cref="BackfillMarker"/>
    /// in CreatedBy. The new code puts a user id or NULL there, and never changes it on
    /// an existing row. Nothing here compares timestamps across rows, so a database that
    /// had no users when the migration ran, or an application clock behind the database
    /// clock, cannot make a row the new code created look like a backfill row.
    /// </para>
    /// <para>
    /// Accepted limits. It decides per row, not per setting: once a row and the old
    /// columns differ, nothing records which side changed. So what a user changed
    /// through an old instance is lost when the new code has since changed any setting
    /// of that user (the whole row counts as the new code's), or when the new code
    /// created the user (the row already has a culture and time zone, and only values
    /// it lacks are filled). And a daily mailing hour that a user an old instance
    /// created set there, then cleared through the new code, comes back. Each needs a
    /// settings change through an old instance during the deploy overlap.
    /// </para>
    /// </summary>
    public static class UserSettingsCatchUp
    {
        /// <summary>
        /// CreatedBy of the rows the AddUserSettings backfill wrote. The migration and
        /// <see cref="Sql"/> spell it out as a literal; keep the three in step.
        /// </summary>
        public const string BackfillMarker = "AddUserSettings";

        public const string Sql = @"
DECLARE @now datetime2 = SYSUTCDATETIME();

-- 1. Users created by an old instance have no row: copy theirs from the old columns.
INSERT dbo.UserSettings ([UserId], [CultureCode], [TimeZone], [DailyMailingHour], [IsTutorialComplete], [Preferences], [Created], [CreatedBy], [Modified], [ModifiedBy])
SELECT u.[Id], u.[CultureCode], u.[TimeZone], u.[DailyMailingHour], u.[IsTutorialComplete],
       CASE WHEN u.[IsAnonymized] = 1 THEN NULL ELSE u.[WallWidgetPreferences] END,
       @now, NULL, @now, NULL
FROM dbo.AspNetUsers u
WHERE NOT EXISTS (SELECT 1 FROM dbo.UserSettings s WHERE s.[UserId] = u.[Id]);

-- 2. A row the new code created on demand (GetOrCreateSettings) for a user an old instance
--    created starts empty, while that user's old columns hold what the old instance wrote.
--    Fill what the row is missing; a value the new code set wins. The new code never writes
--    the old columns, so for a user it created they are empty and the row is left alone.
--    Accepted limits: NULL cannot tell 'never set' from 'cleared by the new code', so a
--    daily mailing hour such a user set through an old instance and then cleared through
--    the new code comes back. And for a user the new code created, the row already has a
--    culture and time zone, so a change to them made through an old instance is not copied.
UPDATE s
SET s.[CultureCode] = COALESCE(s.[CultureCode], u.[CultureCode]),
    s.[TimeZone] = COALESCE(s.[TimeZone], u.[TimeZone]),
    s.[DailyMailingHour] = COALESCE(s.[DailyMailingHour], u.[DailyMailingHour]),
    s.[IsTutorialComplete] = CASE WHEN u.[IsTutorialComplete] = 1 THEN CAST(1 AS bit) ELSE s.[IsTutorialComplete] END,
    s.[Preferences] = COALESCE(s.[Preferences], CASE WHEN u.[IsAnonymized] = 1 THEN NULL ELSE u.[WallWidgetPreferences] END)
FROM dbo.UserSettings s
JOIN dbo.AspNetUsers u ON u.[Id] = s.[UserId]
WHERE (s.[CreatedBy] IS NULL OR s.[CreatedBy] <> N'AddUserSettings')
  AND (u.[CultureCode] IS NOT NULL OR u.[TimeZone] IS NOT NULL OR u.[DailyMailingHour] IS NOT NULL
       OR u.[IsTutorialComplete] = 1 OR u.[WallWidgetPreferences] IS NOT NULL);

-- 3. Only old code writes the old columns, so where they differ from a backfill row the
--    new code never touched (Modified still equals Created), an old instance changed
--    them: copy the change. A row the new code changed keeps the new code's values,
--    all of them: this is per row, so an old-instance change to another setting of that
--    user is not copied (accepted limit).
UPDATE s
SET s.[CultureCode] = u.[CultureCode],
    s.[TimeZone] = u.[TimeZone],
    s.[DailyMailingHour] = u.[DailyMailingHour],
    s.[IsTutorialComplete] = u.[IsTutorialComplete],
    s.[Preferences] = CASE WHEN u.[IsAnonymized] = 1 THEN NULL ELSE u.[WallWidgetPreferences] END,
    s.[Modified] = @now
FROM dbo.UserSettings s
JOIN dbo.AspNetUsers u ON u.[Id] = s.[UserId]
WHERE s.[CreatedBy] = N'AddUserSettings' AND s.[Modified] = s.[Created]
  AND EXISTS (SELECT s.[CultureCode], s.[TimeZone], s.[DailyMailingHour], s.[IsTutorialComplete], s.[Preferences]
              EXCEPT
              SELECT u.[CultureCode], u.[TimeZone], u.[DailyMailingHour], u.[IsTutorialComplete],
                     CASE WHEN u.[IsAnonymized] = 1 THEN NULL ELSE u.[WallWidgetPreferences] END);
";
    }
}
