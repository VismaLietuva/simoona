# Email design foundation

## The one non-obvious thing

`EmailTemplates/HeaderFooter.cshtml` is **generated**. Do not hand-edit its markup — edit
`Design/layout.mjml`, recompile, and commit both files together.

```bash
npx mjml@5 Design/layout.mjml -o Design/layout.html
```

Then fold `Design/layout.html` into `EmailTemplates/HeaderFooter.cshtml`:

1. Escape every `@` as `@@` — Razor treats `@` as a transition and MJML emits `@media`/`@import`.
2. Replace the `<tr>` holding `CONTENT_SLOT` with `@RenderBody()`. It must land directly inside a
   `<tbody>`, because every template body is a run of `<tr>` rows.
3. Replace `SETTINGS_URL_SLOT` with `@Model.UserNotificationSettingsUrl`.
4. Replace `HOME_URL_SLOT` with `@Model.HomeUrl`.
5. Keep the `@model` line and the generated-file comment at the top.

Recompiling is not a CI step on purpose. `appveyor.yml` is a pure .NET pipeline, and adding npm to
it to regenerate one layout that changes twice a year is a bad trade. Checking that the two files
still agree costs nothing, though, so `LayoutIsGeneratedTests` applies the substitutions above to
`Design/layout.html` and compares — hand-editing the `.cshtml` fails the build instead of drifting
silently.

### Why `<mj-font name="Roboto" href="" />` is in the MJML

MJML auto-injects a Google Fonts `<link>` and `@import` for any family name it recognises. That
would be a third-party fetch on every open. Declaring the font with an empty `href` suppresses it —
the whole point of the stack is that recipients use what they already have. After recompiling,
confirm nothing external crept back in:

```bash
grep -o 'https\?://[^"() ]*' EmailTemplates/HeaderFooter.cshtml | sort -u
# expect only http://www.w3.org/1999/xhtml - every real link is a @Model property
```

## Partials

Five tag helpers in `TagHelpers/`, registered via `@addTagHelper *, Shrooms.EmailTemplates` in both
projects' `_ViewImports.cshtml`. `<email-heading>` and `<email-card>` emit table rows and sit
directly in the layout's content slot; the rest go inside a card.

```razor
<email-heading>Kudos notification</email-heading>
<email-card>
    <p style="margin:0;">Body copy.</p>
    <email-quote>User-supplied text.</email-quote>
    <email-details>
        <email-detail label="Location">Vilnius HQ</email-detail>
    </email-details>
    <email-button href="@Model.Url">Primary action</email-button>
</email-card>
```

`EmailTemplates/Design/Showcase.cshtml` renders all five at once. View it at
`/email-preview/Design/Showcase.cshtml`.

## Matching the frontend

`EmailDesign.cs` mirrors the app, not just its palette. Colours are the light-mode tokens computed
from `simoona-nextjs/src/app/globals.css` (email supports neither OKLCH nor `light-dark()`); type
and component values are the Tailwind/shadcn classes the app actually uses:

There is **no header bar**. The email opens with the sidebar's `S/moona` wordmark, linking to
`@Model.HomeUrl`, and the footer is just the notification-preferences link.

`HomeUrl` is the `ClientUrl` setting, filled in by `MailTemplate` rather than by the callers that
build the view models - so the link points at whichever client the sending environment is configured
against (`https://app.simoona.com/` in production) instead of a hardcoded host. Renders that bypass
`MailTemplate`, meaning the golden files, take it from `EmailTemplateSeeds`.

The `SimoonaMark` SVG that sits beside the wordmark in the app is **not** in the email. Gmail and
Outlook strip inline SVG, so it rendered as an empty bordered box for most recipients, and its
draw-then-pulse animation was dead there too — Gmail keeps `<style>` but drops `animation`. The
alternatives were both worse than dropping it: a hosted PNG or GIF would be the only remote fetch in
the whole set, and it needs a publicly reachable URL, which `ClientUrl` is not in development. The
wordmark is plain text, so it renders everywhere and the brand still reads.

| Element | App | Email |
|---|---|---|
| Headline | `text-3xl font-extrabold tracking-tight` | 30px / 800 / `-0.025em` |
| Wordmark | — | 16px / 600 / `--primary`, above the headline |
| Emphasis | `<em class="font-serif font-normal italic">` | Instrument Serif → Georgia italic |
| Body | `text-base` | 16px / 24px |
| Card | `rounded-xl border shadow-sm p-6` | 14px radius, 1px border, `shadow-sm`, 24px |
| Button | Button `default` at size `lg` | `rounded-md` 8px, 14px / 500, 10px 24px |
| Detail label | eyebrow treatment | 10px / 700 / `0.2em` uppercase |

Two deliberate departures:

- **Instrument Serif is a webfont**, so it cannot load in email without a third-party fetch. The
  stack falls back to Georgia, which ships everywhere — the gesture (an italic serif accent in the
  headline) survives even when the exact face does not.
- **Buttons use the `lg` size, not `default`.** `h-9` is 36px; `lg` is 40px and is still the app's
  own token, which suits touch targets in a mail client better.

The page ground is `--muted` rather than the app's white `--background`, so the cards read as cards
in an inbox. That was the Phase 3 token decision and it still holds. Outlook ignores `border-radius`
and `box-shadow`; square, flat corners there are acceptable.

## Golden files

Design changes are reviewed as diffs of `Shrooms.EmailTemplates.Tests/GoldenFiles/`. A baseline only
exists for what `EmailTemplateSeeds` covers, so `EveryCacheKey_HasASeed` fails when a new template
key arrives without a seed — otherwise the template would render in production having never been
looked at. When a change is intended, rewrite the baseline and read the diff before committing:

```bash
UPDATE_EMAIL_GOLDEN=1 dotnet test Shrooms.EmailTemplates.Tests
```
