﻿using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using KushBot.Data;

namespace KushBot.Modules
{
    public class Mole : ModuleBase<SocketCommandContext>
    {
        
        [Command("dig"),Alias("set","digger set")]
        public async Task SetDigger()
        {
            if(!Data.Data.GetPets(Context.User.Id).Contains("2"))
            {
                await ReplyAsync($"{Context.User.Mention} You don't have {Program.Pets[2].Name}, dumb shit");
                return;
            }
            TimeSpan ts;
            ts = Data.Data.GetLootedDigger(Context.User.Id) - DateTime.Now;

            if (Data.Data.GetDiggerState(Context.User.Id) == 1)
            {
                await ReplyAsync($"{Context.User.Mention} Your {Program.Pets[2].Name} is already digging and you don't have 2 of them, retard");
                return;
            }

            if (ts.Ticks > 0)
            {

                if (Data.Data.GetDiggerState(Context.User.Id) == 0)
                {
                    await ReplyAsync($"{Context.User.Mention} However you look at it, your {Program.Pets[2].Name} is too exhausted to dig, you need to wait {ts.Hours:D2}:{ts.Minutes:D2}:{ts.Seconds:D2}");
                    return;
                }
                if (Data.Data.GetDiggerState(Context.User.Id) == -1)
                {
                    await ReplyAsync($"{Context.User.Mention} your {Program.Pets[2].Name} is still unconscious, you should wait {ts.Hours:D2}:{ts.Minutes:D2}:{ts.Seconds:D2}");
                    return;
                }
                await ReplyAsync("Some shit's gone wrong, contact admin");
                return;
            }

            await Data.Data.SaveSetDigger(Context.User.Id, DateTime.Now);
            await Data.Data.SaveDiggerState(Context.User.Id, 1);
            await ReplyAsync($"{Context.User.Mention} You've forced your {Program.Pets[2].Name} to dig \n Use 'kush loot' when you want him to stop");

        }
        [Command("Loot"),Alias("digger loot")]
        public async Task LootDigger()
        {
            if (!Data.Data.GetPets(Context.User.Id).Contains("2"))
            {
                await ReplyAsync($"{Context.User.Mention} You don't have {Program.Pets[2].Name}, dumb shit");
                return;
            }
            if(Data.Data.GetDiggerState(Context.User.Id) != 1)
            {
                await ReplyAsync($"{Context.User.Mention} Your {Program.Pets[2].Name} isn't even digging... \n try using 'kush dig'");
                return;
            }

            TimeSpan length;

            length = DateTime.Now - Data.Data.GetSetDigger(Context.User.Id);

            int minutes = (int)Math.Round(length.TotalMinutes);
            minutes++;

            double deathChance = 2 - (Data.Data.GetPetLevel(Context.User.Id, 2) / (38 + (Data.Data.GetPetLevel(Context.User.Id, 2) / 3.4)));

            double BapsPerMin = 0;
            double BapsGained = 0;

            BapsPerMin = (13 / Math.Pow(deathChance, 0.85));

            int abuseStrength = Data.Data.GetPetAbuseStrength(Context.User.Id, 4);

            bool moleDead = false;

            Random rad = new Random();
            int test = 0;

            for(int i = 0; i < minutes; i++)
            {
                if(deathChance * 100 > rad.Next(0,10000))
                {
                    moleDead = true;
                    test = i;
                    BapsGained = (int)(BapsPerMin * i);
                    break;
                }
                else
                {
                    BapsPerMin = (int)Math.Round(8  / Math.Pow(deathChance, (0.95 + (((double)Data.Data.GetPetLevel(Context.User.Id, 2)) / 300))));
                    //BapsGained += BapsPerMin;
                    if(rad.Next(1,101) < 30 - (Data.Data.GetPetLevel(Context.User.Id,2) / 2))
                    {
                        BapsGained += rad.Next(-1, 1);
                    }
                }
            }

            

            BapsGained = Math.Round(BapsGained, 0);

            if(moleDead)
            {
                Random rnd = new Random();
                int petTier = Data.Data.GetPetTier(Context.User.Id, 2);
                double chanceToSave = petTier * 2 + 3 * Math.Sqrt((double)petTier);
                if(rnd.NextDouble() > chanceToSave / 100)
                {
                    await ReplyAsync($"{Context.User.Mention} Checking in after {minutes} minutes, you've found your {Program.Pets[2].Name} unconscious");
                }
                else
                {
                    await ReplyAsync($"{Context.User.Mention} Checking in after {minutes} minutes, you've found your {Program.Pets[2].Name} unconscious, but he seems to have left a pile" +
                        $" of {BapsGained} baps");
                    await Data.Data.SaveBalance(Context.User.Id, (int)BapsGained, false);
                }
                await Data.Data.SaveLootedDigger(Context.User.Id, DateTime.Now.AddHours(1).AddMinutes(35 + -1 * (Data.Data.GetPetLevel(Context.User.Id,2) / 2) + 1 - (abuseStrength * 12)));
                await Data.Data.SaveDiggerState(Context.User.Id, -1);
            }
            else
            {
                BapsGained += minutes * BapsPerMin;
				for (int i = 0; i < abuseStrength; i++)
				{
					BapsGained *= 1.2;
				}
                BapsGained = Math.Round(BapsGained, 0);
                await Data.Data.SaveBalance(Context.User.Id, (int)BapsGained, false);
                await ReplyAsync($"{Context.User.Mention} After pulling your {Program.Pets[2].Name} out of it's hole he hands you **{BapsGained}** Baps which he got in {minutes} minutes");
                await Data.Data.SaveLootedDigger(Context.User.Id, DateTime.Now.AddHours(1).AddMinutes(50 + - (Data.Data.GetPetAbuseStrength(Context.User.Id, 3) * 10) - (-1 * (Data.Data.GetPetLevel(Context.User.Id, 2) / 2) + 1)));
                await Data.Data.SaveDiggerState(Context.User.Id, 0);
            }

        }

    }
}
