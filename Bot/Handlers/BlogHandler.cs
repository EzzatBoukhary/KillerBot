using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bot.Configuration;
using Bot.Features.Blogs;
using Discord;
using Discord.WebSocket;
using Newtonsoft.Json;

namespace Bot.Handlers
{
    public class BlogHandler
    {
        private readonly JsonDataStorage _jsonDataStorage;
        private readonly string blogFile = "blogs.json";
        public BlogHandler(JsonDataStorage jsonDataStorage)
        {
            this._jsonDataStorage = jsonDataStorage;
        }

        public Embed SubscribeToBlog(ulong userId, string blogname)
        {
            var blogs = _jsonDataStorage.RestoreObject<List<BlogItem>>(blogFile);

            var blog = blogs.FirstOrDefault(k => k.Name == blogname);

            if (blog != null)
            {
                if (!blog.Subscribers.Contains(userId))
                {
                    blog.Subscribers.Add(userId);

                    _jsonDataStorage.StoreObject(blogs, blogFile, Formatting.Indented);

                    return EmbedHandler.CreateEmbed("Blog", $"You now follow \"{blogname}\" \nAlso, make sure to have dms enabled for the bot to DM you any new posts from this blog.", EmbedHandler.EmbedMessageType.Success);
                }
                else
                {
                    return EmbedHandler.CreateEmbed("Blog :x:", "You already follow this blog", EmbedHandler.EmbedMessageType.Info);
                }
            }
            else
            {
                return EmbedHandler.CreateEmbed("Blog :x:", $"There is no Blog with the name {blogname}", EmbedHandler.EmbedMessageType.Error);
            }
        }

        public Embed UnSubscribeToBlog(ulong userId, string blogname)
        {
            var blogs = _jsonDataStorage.RestoreObject<List<BlogItem>>(blogFile);

            var blog = blogs.FirstOrDefault(k => k.Name == blogname);

            if (blog != null)
            {
                if (blog.Subscribers.Contains(userId))
                {
                    blog.Subscribers.Remove(userId);

                    _jsonDataStorage.StoreObject(blogs, blogFile, Formatting.Indented);

                    return EmbedHandler.CreateEmbed("Blog", "You stopped following this blog", EmbedHandler.EmbedMessageType.Success);
                }
                else
                {
                    return EmbedHandler.CreateEmbed("Blog :x:", "You don't follow this blog", EmbedHandler.EmbedMessageType.Info);
                }
            }
            else
            {
                return EmbedHandler.CreateEmbed("Blog :x:", $"There is no Blog with the name {blogname}", EmbedHandler.EmbedMessageType.Error);
            }
        }
      
        public async Task ReactionAdded(SocketReaction reaction)
        {
            var msgList = Global.MessagesIdToTrack ?? new Dictionary<ulong, string>();
            if (msgList.ContainsKey(reaction.MessageId))
            {
                if (reaction.Emote.Name == "➕")
                {
                    var item = msgList.FirstOrDefault(k => k.Key == reaction.MessageId);
                    var embed = SubscribeToBlog(reaction.User.Value.Id, item.Value);
                }
            }
        }
       
    }
} 
