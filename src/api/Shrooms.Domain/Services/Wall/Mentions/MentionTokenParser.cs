using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Shrooms.Domain.Services.Wall.Mentions
{
    /// <summary>
    /// Reads the mention tokens the client writes into a message body:
    /// <c>@[Jane Doe](user:9f3e...)</c> and <c>@[Marketing](group:12)</c>.
    /// The '@' sits outside the brackets so a reader that knows nothing about the
    /// token still shows "@Jane Doe", and so a mention can never be confused with
    /// an ordinary markdown link the author typed by hand.
    /// Must stay in step with src/lib/mentions.ts in the web client.
    /// </summary>
    public static class MentionTokenParser
    {
        public const int LabelMaxLength = 100;

        private const string GuidPattern =
            "[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}";

        private static readonly Regex Pattern = new Regex(
            @"@\[(?<label>[^\]\n]{1," + LabelMaxLength + @"})\]\((?:user:(?<user>" + GuidPattern + @")|group:(?<group>\d{1,9}))\)",
            RegexOptions.Compiled);

        public static IReadOnlyCollection<MentionToken> Parse(string messageBody)
        {
            var tokens = new List<MentionToken>();

            if (string.IsNullOrEmpty(messageBody))
            {
                return tokens;
            }

            foreach (Match match in Pattern.Matches(messageBody))
            {
                tokens.Add(ToToken(match));
            }

            return tokens;
        }

        public static string Replace(string messageBody, Func<MentionToken, string> render)
        {
            return string.IsNullOrEmpty(messageBody)
                ? messageBody
                : Pattern.Replace(messageBody, match => render(ToToken(match)));
        }

        private static MentionToken ToToken(Match match)
        {
            var user = match.Groups["user"];

            return new MentionToken
            {
                Kind = user.Success ? MentionKind.User : MentionKind.Group,
                Label = match.Groups["label"].Value,
                UserId = user.Success ? user.Value : null,
                GroupId = user.Success
                    ? 0
                    : int.Parse(match.Groups["group"].Value, CultureInfo.InvariantCulture)
            };
        }
    }
}
