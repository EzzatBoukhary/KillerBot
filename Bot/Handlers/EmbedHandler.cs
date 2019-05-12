using Discord;

namespace Bot.Handlers
{
    public static class EmbedHandler
    {
        /// <summary>
        /// Create a new embed
        /// </summary>
        /// <param name="title">Title of the embed</param>
        /// <param name="body">Embed content</param>
        /// <param name="type">Type of the Embed (Error, Info, Exception, Success) -> Sets the color</param>
        /// <param name="withTimeStamp">Adds the current Timestamp to the embed</param>
        /// <returns></returns>
        public static Embed CreateEmbed(string title, string body, EmbedMessageType type, bool withTimeStamp = false)
        {
            var embed = new EmbedBuilder();
            embed.WithTitle(title);
            embed.WithDescription(body);

            switch (type)
            {
                case EmbedMessageType.Info:
                    embed.WithColor(new Color(52, 152, 219));
                    break;
                case EmbedMessageType.Success:
                    embed.WithColor(new Color(22, 160, 133));
                    break;
                case EmbedMessageType.Error:
                    embed.WithColor(new Color(192, 57, 43));
                    break;
                case EmbedMessageType.Exception:
                    embed.WithColor(new Color(230, 126, 34));
                    break;
                default:
                    embed.WithColor(new Color(149, 165, 166));
                    break;
            }

            if (withTimeStamp)
            {
                embed.WithCurrentTimestamp();
            }

            return embed.Build();
        }

        public static Embed CreateBlogEmbed(string title, string body, string subscribers, EmbedMessageType type, bool withTimeStamp = false)
        {
            var embed = new EmbedBuilder();
            embed.WithTitle($"Blog: {title}");
            embed.WithDescription(body);
            embed.AddField("Subscribers", subscribers);

            embed.WithFooter("+ to subscribe");

            switch (type)
            {
                case EmbedMessageType.Info:
                    embed.WithColor(new Color(52, 152, 219));
                    break;
                case EmbedMessageType.Success:
                    embed.WithColor(new Color(22, 160, 133));
                    break;
                case EmbedMessageType.Error:
                    embed.WithColor(new Color(192, 57, 43));
                    break;
                case EmbedMessageType.Exception:
                    embed.WithColor(new Color(230, 126, 34));
                    break;
                default:
                    embed.WithColor(new Color(149, 165, 166));
                    break;
            }

            if (withTimeStamp)
            {
                embed.WithCurrentTimestamp();
            }

            return embed.Build();
        }

        public enum EmbedMessageType
        {
            Success = 0,
            Info = 10,
            Error = 20,
            Exception = 30
        }
    }
}
