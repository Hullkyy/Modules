using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using HulkysBot1.Core.UserAccounts;

namespace HulkysBot1.Modules
{
    public class Commands : ModuleBase<SocketCommandContext>
    {
        public object Utilities { get; private set; }

        [Command("Stats")]
        public async Task UserStats([Remainder]string arg = "")
        {
            SocketUser target = null;
            var mentionedUser = Context.Message.MentionedUsers.FirstOrDefault();
            target = mentionedUser ?? Context.User;

            var account = UserAccounts.GetAccount(target);
            await Context.Channel.SendMessageAsync($"{target.Mention} has {account.XP} XP and {account.Points} points");
        }

        [Command("modifyXP")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task AddXP(IGuildUser user, int xp)
        {
            var account = UserAccounts.GetAccount(Context.User);

            {
                if (!UserIsAdministrator((SocketGuildUser)Context.User))
                {
                    await Context.Channel.SendMessageAsync(":x: You need the Administrator role to do that!" + " " + Context.User.Mention);
                    return;
                }
                var dmChannel = await Context.User.GetOrCreateDMChannelAsync();
                await dmChannel.SendMessageAsync(HulkysBot1.Utilities.GetAlert("ADMINISTRATOR"));
            }

            bool UserIsAdministrator(SocketGuildUser user)
            {
                string targetRoleName = "Administrator";
                var result = from r in user.Guild.Roles
                             where r.Name == targetRoleName
                             select r.Id;
                ulong roleID = result.FirstOrDefault();
                if (roleID == 0) return false;
                var targetRole = user.Guild.GetRole(roleID);
                return user.Roles.Contains(targetRole);
            }

            if (xp > 0)
            {
                account.XP += (uint)xp;
                await Context.Channel.SendMessageAsync($"{Context.User.Mention} was granted {xp} XP by {Context.User.Mention}. :eggplant:");
            }
            else if (xp * -1 > account.XP)
            {
                await Context.Channel.SendMessageAsync("You can't remove more XP than you have! :thinking:");
                return;
            }
            else
            {
                account.XP += (uint)xp;
                await Context.Channel.SendMessageAsync($"{Context.User.Mention} removed {xp} XP from {Context.User.Mention}. :broken_heart:");
            }
            UserAccounts.SaveAccounts();
        }

        [Command("ACI")]
        
        public async Task AccountInfo()
        {
            var gUser = Context.User as SocketGuildUser;
            var embB = new EmbedBuilder()
                .AddField($"Profile - {gUser.Username}", $"**Mention:** {gUser.Mention}\n" + $"**Nickname:** {gUser.Nickname ?? "``"}")
                .AddField("Details", $"**Status:** {gUser.Status}\n" +
                                    $"\n" +
                                    $"**Guild:** {gUser.Guild}\n" +
                                    $"\n" +
                                    $"**Roles:** {gUser.Roles}\n")
                .WithFooter("Account creation date: " + Context.User.CreatedAt);

            await Context.Channel.SendMessageAsync("", false, embB.Build());


        }

        [Command("Pick")]
        public async Task PickAChoice([Remainder]string message)
        {
            string[] options = message.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);

            Random r = new Random();
            string selection = options[r.Next(0, options.Length)];

            var embed = new EmbedBuilder();
            embed.WithTitle("Choice for " + Context.User.Username);
            embed.WithDescription(selection);
            embed.WithColor(new Color(0, 255, 0));


            await Context.Channel.SendMessageAsync("", false, embed);
            DataStorage.AddPairToStorage(Context.User.Username + DateTime.Now.ToLongTimeString(), selection);

        }

        [Command("Secret")]
        public async Task RevealSecret([Remainder]string arg = "")
        {
            if (!UserIsSecret((SocketGuildUser)Context.User))
            {
                await Context.Channel.SendMessageAsync(":x: You need the Secret role to do that!" + " " + Context.User.Mention);
                return;
            }
            var dmChannel = await Context.User.GetOrCreateDMChannelAsync();
            await dmChannel.SendMessageAsync(HulkysBot1.Utilities.GetAlert("SECRET"));
        }
        
                private bool UserIsSecret(SocketGuildUser user)
                {
                    string targetRoleName = "Secret";
                    var result = from r in user.Guild.Roles
                                 where r.Name == targetRoleName
                                 select r.Id;
                    ulong roleID = result.FirstOrDefault();
                    if (roleID == 0) return false;
                    var targetRole = user.Guild.GetRole(roleID);
                    return user.Roles.Contains(targetRole);
                }


        [Command("Slothy")]
        public async Task SexySloothy()
        {

            var embed = new EmbedBuilder();
            embed.WithImageUrl("https://3.bp.blogspot.com/-NW-FQOm14I4/Tt905_JVVWI/AAAAAAAAAkI/-mX7Pm527Ps/s1600/Sloth-8.jpg" + Context.User.Username);
            embed.WithColor(new Color(0, 255, 0));


            await Context.Channel.SendMessageAsync("", false, embed);
        }


        [Command("Commands")]
        public async Task ShowCommands()
        {
            var gUser = Context.User as SocketGuildUser;
            var embB = new EmbedBuilder()
                .WithColor(new Color(0, 255, 0))
                .AddField("Commands: ", $"**!Slothy:** \n" +
                                    $"Shows an image of a sexy sloth. \n" +
                                    $"**!ACI:** \n" +
                                    $"Shows clients stats/details. \n" +
                                    $"**!Pick: ** \n" +
                                    $"Picks between 1-10 given selections e.g. 1|2|3 \n" +
                                    $"**!Secret:** \n" +
                                    $"Work in progress. \n");
            


            await Context.Channel.SendMessageAsync("", false, embB.Build());
        }

        [Command("data")]
        public async Task GetDate()
        {
            await Context.Channel.SendMessageAsync("Data Has " + DataStorage.GetPairsCount() + " pair(s).");
        }
    }
}
