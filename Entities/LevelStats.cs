using System;
using System.Collections.Generic;
using System.Drawing;
using LiteDB;

namespace FallGuysStats {
    public class RoundInfo : IComparable<RoundInfo> {
        public ObjectId ID { get; set; }
        public int Profile { get; set; }
        public string Name { get; set; }
        public int ShowID { get; set; }
        public string ShowNameId { get; set; }
        public bool UseShareCode { get; set; }
        public string CreativeShareCode { get; set; }
        public string CreativeStatus { get; set; }
        public string CreativeAuthor { get; set; }
        public string CreativeOnlinePlatformId { get; set; }
        //public string CreativeNicknameContentId { get; set; }
        //public string CreativeNameplateContentId { get; set; }
        public int CreativeVersion { get; set; }
        //public string CreativeStatus { get; set; }
        public string CreativeTitle { get; set; }
        public string CreativeDescription { get; set; }
        public int CreativeMaxPlayer { get; set; }
        public string CreativePlatformId { get; set; }
        public DateTime CreativeLastModifiedDate { get; set; } = DateTime.MinValue;
        public int CreativePlayCount { get; set; }
        public string SessionId { get; set; }
        public int Round { get; set; }
        public int Position { get; set; }
        public int? Score { get; set; }
        public int Tier { get; set; }
        public bool Qualified { get; set; }
        public bool AbandonShow { get; set; }
        public int Kudos { get; set; }
        public int Players { get; set; }
        public int PlayersPs4 { get; set; }
        public int PlayersPs5 { get; set; }
        public int PlayersXb1 { get; set; }
        public int PlayersXsx { get; set; }
        public int PlayersSw { get; set; }
        public int PlayersPc { get; set; }
        public int PlayersBots { get; set; }
        public int PlayersEtc { get; set; }
        public bool IsFinal { get; set; }
        public bool IsTeam { get; set; }
        public bool InParty { get; set; }
        public bool PrivateLobby { get; set; }
        public DateTime Start { get; set; } = DateTime.MinValue;
        public DateTime End { get; set; } = DateTime.MinValue;
        public DateTime? Finish { get; set; } = null;
        public bool Crown { get; set; }

        public DateTime StartLocal;
        public DateTime EndLocal;
        public DateTime? FinishLocal;
        public DateTime ShowStart = DateTime.MinValue;
        public DateTime ShowEnd = DateTime.MinValue;
        public int GameDuration;
        public string SceneName;
        public bool Playing;
        private bool setLocalTime;

        public void ToLocalTime() {
            if (this.setLocalTime) { return; }
            this.setLocalTime = true;

            this.StartLocal = this.Start.ToLocalTime();
            this.EndLocal = this.End.ToLocalTime();
            if (this.Finish.HasValue) {
                this.FinishLocal = this.Finish.Value.ToLocalTime();
            }
        }
        public void VerifyName() {
            if (string.IsNullOrEmpty(this.SceneName)) { return; }

            if (LevelStats.SceneToRound.TryGetValue(this.SceneName, out string roundName)) {
                this.Name = roundName;
            }
        }
        public string VerifiedName() {
            return string.IsNullOrEmpty(this.SceneName)
                   ? this.Name
                   : LevelStats.SceneToRound.TryGetValue(this.SceneName, out string roundName) ? roundName : this.Name;
        }
        public override string ToString() {
            return $"{this.Name}: Round={this.Round} Position={this.Position} Duration={this.End - this.Start} Kudos={this.Kudos}";
        }
        public override bool Equals(object obj) {
            return obj is RoundInfo info
                   && info.End == this.End
                   && info.Finish == this.Finish
                   && info.InParty == this.InParty
                   && info.Kudos == this.Kudos
                   && info.Players == this.Players
                   && info.PlayersPs4 == this.PlayersPs4
                   && info.PlayersPs5 == this.PlayersPs5
                   && info.PlayersXb1 == this.PlayersXb1
                   && info.PlayersXsx == this.PlayersXsx
                   && info.PlayersSw == this.PlayersSw
                   && info.PlayersPc == this.PlayersPc
                   && info.PlayersBots == this.PlayersBots
                   && info.PlayersEtc == this.PlayersEtc
                   && info.Position == this.Position
                   && info.Qualified == this.Qualified
                   && info.AbandonShow == this.AbandonShow
                   && info.Round == this.Round
                   && info.Score == this.Score
                   && info.ShowID == this.ShowID
                   && info.Start == this.Start
                   && info.Tier == this.Tier
                   && info.Name == this.Name
                   && info.SessionId == this.SessionId
                   && info.UseShareCode == this.UseShareCode;
        }
        public override int GetHashCode() {
            return Name.GetHashCode() ^ ShowID ^ Round;
        }
        public int CompareTo(RoundInfo other) {
            int showCompare = ShowID.CompareTo(other.ShowID);
            return showCompare != 0 ? showCompare : Round.CompareTo(other.Round);
        }
    }
    public enum QualifyTier {
        Pink,
        Gold,
        Silver,
        Bronze
    }
    public class LevelStats {
        public static Dictionary<string, LevelStats> ALL = new Dictionary<string, LevelStats>(StringComparer.OrdinalIgnoreCase) {
            { "round_biggestfan",                 new LevelStats("Big Fans", LevelType.Race, BestRecordType.Fastest, false, false, 2, Properties.Resources.round_big_fans_icon, Properties.Resources.round_big_fans_big_icon) },
            { "round_satellitehoppers_almond",    new LevelStats("Cosmic Highway", LevelType.Race, BestRecordType.Fastest, false, false, 8, Properties.Resources.round_cosmic_highway_icon, Properties.Resources.round_cosmic_highway_big_icon) },
            { "round_gauntlet_02",                new LevelStats("Dizzy Heights", LevelType.Race, BestRecordType.Fastest, false, false, 1, Properties.Resources.round_dizzy_heights_icon, Properties.Resources.round_dizzy_heights_big_icon) },
            { "round_door_dash",                  new LevelStats("Door Dash", LevelType.Race, BestRecordType.Fastest, false, false, 1, Properties.Resources.round_door_dash_icon, Properties.Resources.round_door_dash_big_icon) },
            { "round_iceclimb",                   new LevelStats("Freezy Peak", LevelType.Race, BestRecordType.Fastest, false, false, 3, Properties.Resources.round_freezy_peak_icon, Properties.Resources.round_freezy_peak_big_icon) },
            { "round_dodge_fall",                 new LevelStats("Fruit Chute", LevelType.Race, BestRecordType.Fastest, false, false, 1, Properties.Resources.round_fruit_chute_icon, Properties.Resources.round_fruit_chute_big_icon) },
            { "round_see_saw_360",                new LevelStats("Full Tilt", LevelType.Race, BestRecordType.Fastest, false, false, 6, Properties.Resources.round_full_tilt_icon, Properties.Resources.round_full_tilt_big_icon) },
            { "round_chompchomp",                 new LevelStats("Gate Crash", LevelType.Race, BestRecordType.Fastest, false, false, 1, Properties.Resources.round_gate_crash_icon, Properties.Resources.round_gate_crash_big_icon) },
            { "round_gauntlet_01",                new LevelStats("Hit Parade", LevelType.Race, BestRecordType.Fastest, false, false, 1, Properties.Resources.round_hit_parade_icon, Properties.Resources.round_hit_parade_big_icon) },
            { "round_gauntlet_04",                new LevelStats("Knight Fever", LevelType.Race,BestRecordType.Fastest, false, false, 2, Properties.Resources.round_knight_fever_icon, Properties.Resources.round_knight_fever_big_icon) },
            { "round_drumtop",                    new LevelStats("Lily Leapers", LevelType.Race, BestRecordType.Fastest, false, false, 5, Properties.Resources.round_lily_leapers_icon, Properties.Resources.round_lily_leapers_big_icon) },
            { "round_gauntlet_08",                new LevelStats("Party Promenade", LevelType.Race, BestRecordType.Fastest, false, false, 6, Properties.Resources.round_party_promenade_icon, Properties.Resources.round_party_promenade_big_icon) },
            { "round_pipedup_s6_launch",          new LevelStats("Pipe Dream", LevelType.Race, BestRecordType.Fastest, false, false, 6, Properties.Resources.round_pipe_dream_icon, Properties.Resources.round_pipe_dream_big_icon) },
            { "round_follow_the_line",            new LevelStats("Puzzle Path", LevelType.Race, BestRecordType.Fastest, false, false, 9, Properties.Resources.round_puzzle_path_icon, Properties.Resources.round_puzzle_path_big_icon) },
            { "round_tunnel_race",                new LevelStats("Roll On", LevelType.Race, BestRecordType.Fastest, false, false, 4, Properties.Resources.round_roll_on_icon, Properties.Resources.round_roll_on_big_icon) },
            { "round_see_saw",                    new LevelStats("See Saw", LevelType.Race, BestRecordType.Fastest, false, false, 1, Properties.Resources.round_see_saw_icon, Properties.Resources.round_see_saw_big_icon) },
            { "round_shortcircuit",               new LevelStats("Short Circuit", LevelType.Race, BestRecordType.Fastest, false, false, 4, Properties.Resources.round_short_circuit_icon, Properties.Resources.round_short_circuit_big_icon) },
            { "round_skeefall",                   new LevelStats("Ski Fall", LevelType.Race, BestRecordType.Fastest, false, false, 3, Properties.Resources.round_ski_fall_icon, Properties.Resources.round_ski_fall_big_icon) },
            { "round_gauntlet_06",                new LevelStats("Skyline Stumble", LevelType.Race, BestRecordType.Fastest, false, false, 4, Properties.Resources.round_skyline_stumble_icon, Properties.Resources.round_skyline_stumble_big_icon) },
            { "round_lava",                       new LevelStats("Slime Climb", LevelType.Race, BestRecordType.Fastest, false, false, 1, Properties.Resources.round_slime_climb_icon, Properties.Resources.round_slime_climb_big_icon) },
            { "round_gauntlet_10_almond",         new LevelStats("Space Race", LevelType.Race, BestRecordType.Fastest, false, false, 8, Properties.Resources.round_space_race_icon, Properties.Resources.round_space_race_big_icon) },
            { "round_short_circuit_2_symphony_launch_show", new LevelStats("Speed Circuit", LevelType.Race, BestRecordType.Fastest, false, false, 7, Properties.Resources.round_speed_circuit_icon, Properties.Resources.round_speed_circuit_big_icon) },
            { "round_slide_chute",                new LevelStats("Speed Slider", LevelType.Race, BestRecordType.Fastest, false, false, 9, Properties.Resources.round_speed_slider_icon, Properties.Resources.round_speed_slider_big_icon) },
            { "round_starlink_almond",            new LevelStats("Starchart", LevelType.Race, BestRecordType.Fastest, false, false, 8, Properties.Resources.round_starchart_icon, Properties.Resources.round_starchart_big_icon) },
            { "round_slimeclimb_2",               new LevelStats("The Slimescraper", LevelType.Race, BestRecordType.Fastest, false, false, 4, Properties.Resources.round_the_slimescraper_icon, Properties.Resources.round_the_slimescraper_big_icon) },
            { "round_gauntlet_03",                new LevelStats("The Whirlygig", LevelType.Race, BestRecordType.Fastest, false, false, 1, Properties.Resources.round_the_whirlygig_icon, Properties.Resources.round_the_whirlygig_big_icon) },
            { "round_tip_toe",                    new LevelStats("Tip Toe", LevelType.Race, BestRecordType.Fastest, false, false, 1, Properties.Resources.round_tip_toe_icon, Properties.Resources.round_tip_toe_big_icon) },
            { "round_gauntlet_09_symphony_launch_show", new LevelStats("Track Attack", LevelType.Race, BestRecordType.Fastest, false, false, 7, Properties.Resources.round_track_attack_icon, Properties.Resources.round_track_attack_big_icon) },
            { "round_gauntlet_07",                new LevelStats("Treetop Tumble", LevelType.Race, BestRecordType.Fastest, false, false, 5, Properties.Resources.round_treetop_tumble_icon, Properties.Resources.round_treetop_tumble_big_icon) },
            { "round_gauntlet_05",                new LevelStats("Tundra Run", LevelType.Race, BestRecordType.Fastest, false, false, 3, Properties.Resources.round_tundra_run_icon, Properties.Resources.round_tundra_run_big_icon) },
            { "round_wall_guys",                  new LevelStats("Wall Guys", LevelType.Race, BestRecordType.Fastest, false, false, 2, Properties.Resources.round_wall_guys_icon, Properties.Resources.round_wall_guys_big_icon) },

            { "round_fruitpunch_s4_show",         new LevelStats("Big Shots", LevelType.Survival, BestRecordType.Longest, false, false, 4, Properties.Resources.round_big_shots_icon, Properties.Resources.round_big_shots_big_icon) },
            { "round_blastballruins",             new LevelStats("Blastlantis", LevelType.Survival, BestRecordType.Longest, false, false, 9, Properties.Resources.round_blastlantis_icon, Properties.Resources.round_blastlantis_big_icon) },
            { "round_block_party",                new LevelStats("Block Party", LevelType.Survival, BestRecordType.Longest, false, false, 1, Properties.Resources.round_block_party_icon, Properties.Resources.round_block_party_big_icon) },
            { "round_hoverboardsurvival_s4_show", new LevelStats("Hoverboard Heroes", LevelType.Survival, BestRecordType.Fastest, false, false, 4, Properties.Resources.round_hoverboard_heroes_icon, Properties.Resources.round_hoverboard_heroes_big_icon) },
            { "round_hoverboardsurvival2_almond", new LevelStats("Hyperdrive Heroes", LevelType.Survival, BestRecordType.Fastest, false, false, 8, Properties.Resources.round_hyperdrive_heroes_icon, Properties.Resources.round_hyperdrive_heroes_big_icon) },
            { "round_jump_club",                  new LevelStats("Jump Club", LevelType.Survival, BestRecordType.Longest, false, false, 1, Properties.Resources.round_jump_club_icon, Properties.Resources.round_jump_club_big_icon) },
            { "round_tunnel",                     new LevelStats("Roll Out", LevelType.Survival, BestRecordType.Longest, false, false, 1, Properties.Resources.round_roll_out_icon, Properties.Resources.round_roll_out_big_icon) },
            { "round_snowballsurvival",           new LevelStats("Snowball Survival", LevelType.Survival, BestRecordType.Longest, false, false, 3, Properties.Resources.round_snowball_survival_icon, Properties.Resources.round_snowball_survival_big_icon) },
            { "round_robotrampage_arena_2",       new LevelStats("Stompin' Ground", LevelType.Survival, BestRecordType.Longest, false, false, 5, Properties.Resources.round_stompin_ground_icon, Properties.Resources.round_stompin_ground_big_icon) },
            { "round_spin_ring_symphony_launch_show", new LevelStats("The Swiveller", LevelType.Survival, BestRecordType.Longest, false, false, 7, Properties.Resources.round_the_swiveller_icon, Properties.Resources.round_the_swiveller_big_icon) },

            { "round_airtime",                    new LevelStats("Airtime", LevelType.Hunt, BestRecordType.Fastest, false, false, 6, Properties.Resources.round_airtime_icon, Properties.Resources.round_airtime_big_icon) },
            { "round_bluejay",                    new LevelStats("Bean Hill Zone", LevelType.Hunt, BestRecordType.Fastest, false, false, 7, Properties.Resources.round_bean_hill_zone_icon, Properties.Resources.round_bean_hill_zone_big_icon) },
            { "round_hoops_revenge_symphony_launch_show", new LevelStats("Bounce Party", LevelType.Hunt, BestRecordType.Fastest, false, false, 7, Properties.Resources.round_bounce_party_icon, Properties.Resources.round_bounce_party_big_icon) },
            { "round_king_of_the_hill",           new LevelStats("Bubble Trouble", LevelType.Hunt, BestRecordType.Fastest, false, false, 5, Properties.Resources.round_bubble_trouble_icon, Properties.Resources.round_bubble_trouble_big_icon) },
            { "round_1v1_button_basher",          new LevelStats("Button Bashers", LevelType.Hunt, BestRecordType.HighScore, false, false, 4, Properties.Resources.round_button_bashers_icon, Properties.Resources.round_button_bashers_big_icon) },
            { "round_ffa_button_bashers_squads_almond", new LevelStats("Frantic Factory", LevelType.Hunt, BestRecordType.Fastest, false, false, 8, Properties.Resources.round_frantic_factory_icon, Properties.Resources.round_frantic_factory_big_icon) },
            { "round_slippy_slide",               new LevelStats("Hoop Chute", LevelType.Hunt, BestRecordType.Fastest, false, false, 9, Properties.Resources.round_hoop_chute_icon, Properties.Resources.round_hoop_chute_big_icon) },
            { "round_hoops_blockade_solo",        new LevelStats("Hoopsie Legends", LevelType.Hunt, BestRecordType.Fastest, false, false, 2, Properties.Resources.round_hoopsie_legends_icon, Properties.Resources.round_hoopsie_legends_big_icon) },
            { "round_follow-the-leader_s6_launch",new LevelStats("Leading Light", LevelType.Hunt, BestRecordType.Fastest, false, false, 6, Properties.Resources.round_leading_light_icon, Properties.Resources.round_leading_light_big_icon) },
            { "round_penguin_solos",              new LevelStats("Pegwin Pool Party", LevelType.Hunt, BestRecordType.Fastest, false, false, 5, Properties.Resources.round_pegwin_pool_party_icon, Properties.Resources.round_pegwin_pool_party_big_icon) },
            { "round_tail_tag",                   new LevelStats("Tail Tag", LevelType.Hunt, BestRecordType.HighScore, false, false, 1, Properties.Resources.round_tail_tag_icon, Properties.Resources.round_tail_tag_big_icon) },
            { "round_1v1_volleyfall_symphony_launch_show", new LevelStats("Volleyfall", LevelType.Hunt, BestRecordType.HighScore, false, false, 7, Properties.Resources.round_volleyfall_icon, Properties.Resources.round_volleyfall_big_icon) },

            { "round_match_fall",                 new LevelStats("Perfect Match", LevelType.Logic, BestRecordType.Longest, false, false, 1, Properties.Resources.round_perfect_match_icon, Properties.Resources.round_perfect_match_big_icon) },
            { "round_pixelperfect_almond",        new LevelStats("Pixel Painters", LevelType.Logic, BestRecordType.Fastest, false, false, 8, Properties.Resources.round_pixel_painters_icon, Properties.Resources.round_pixel_painters_big_icon) },
            { "round_fruit_bowl",                 new LevelStats("Sum Fruit", LevelType.Logic, BestRecordType.Longest, false, false, 5, Properties.Resources.round_sum_fruit_icon, Properties.Resources.round_sum_fruit_big_icon) },

            { "round_basketfall_s4_show",         new LevelStats("Basketfall", LevelType.Team, BestRecordType.HighScore, false, false, 4, Properties.Resources.round_basketfall_icon, Properties.Resources.round_basketfall_big_icon) },
            { "round_egg_grab",                   new LevelStats("Egg Scramble", LevelType.Team, BestRecordType.HighScore, false, false, 1, Properties.Resources.round_egg_scramble_icon, Properties.Resources.round_egg_scramble_big_icon) },
            { "round_egg_grab_02",                new LevelStats("Egg Siege", LevelType.Team, BestRecordType.HighScore, false, false, 2, Properties.Resources.round_egg_siege_icon, Properties.Resources.round_egg_siege_big_icon) },
            { "round_fall_ball_60_players",       new LevelStats("Fall Ball", LevelType.Team, BestRecordType.HighScore, false, false, 1, Properties.Resources.round_fall_ball_icon, Properties.Resources.round_fall_ball_big_icon) },
            { "round_ballhogs",                   new LevelStats("Hoarders", LevelType.Team, BestRecordType.HighScore, false, false, 1, Properties.Resources.round_hoarders_icon, Properties.Resources.round_hoarders_big_icon) },
            { "round_hoops",                      new LevelStats("Hoopsie Daisy", LevelType.Team, BestRecordType.HighScore, false, false, 1, Properties.Resources.round_hoopsie_daisy_icon, Properties.Resources.round_hoopsie_daisy_big_icon) },
            { "round_jinxed",                     new LevelStats("Jinxed", LevelType.Team, BestRecordType.Fastest, false, false, 1, Properties.Resources.round_jinxed_icon, Properties.Resources.round_jinxed_big_icon) },
            { "round_chicken_chase",              new LevelStats("Pegwin Pursuit", LevelType.Team, BestRecordType.HighScore, false, false, 3, Properties.Resources.round_pegwin_pursuit_icon, Properties.Resources.round_pegwin_pursuit_big_icon) },
            { "round_territory_control_s4_show",  new LevelStats("Power Trip", LevelType.Team, BestRecordType.HighScore, false, false, 4, Properties.Resources.round_power_trip_icon, Properties.Resources.round_power_trip_big_icon) },
            { "round_rocknroll",                  new LevelStats("Rock 'n' Roll", LevelType.Team, BestRecordType.Fastest, false, false, 1, Properties.Resources.round_rock_n_roll_icon, Properties.Resources.round_rock_n_roll_big_icon) },
            { "round_snowy_scrap",                new LevelStats("Snowy Scrap", LevelType.Team, BestRecordType.Fastest, false, false, 3, Properties.Resources.round_snowy_scrap_icon, Properties.Resources.round_snowy_scrap_big_icon) },
            { "round_conveyor_arena",             new LevelStats("Team Tail Tag", LevelType.Team, BestRecordType.Fastest, false, false, 1, Properties.Resources.round_team_tail_tag_icon, Properties.Resources.round_team_tail_tag_big_icon) },

            { "round_blastball_arenasurvival_symphony_launch_show", new LevelStats("Blast Ball", LevelType.Survival, BestRecordType.Longest, false, true, 7, Properties.Resources.round_blast_ball_icon, Properties.Resources.round_blast_ball_big_icon) },
            { "round_fall_mountain_hub_complete", new LevelStats("Fall Mountain", LevelType.Race, BestRecordType.Fastest, false, true, 1, Properties.Resources.round_fall_mountain_icon, Properties.Resources.round_fall_mountain_big_icon) },
            { "round_floor_fall",                 new LevelStats("Hex-A-Gone", LevelType.Survival, BestRecordType.Longest, false, true, 1, Properties.Resources.round_hex_a_gone_icon, Properties.Resources.round_hex_a_gone_big_icon) },
            { "round_hexaring_symphony_launch_show", new LevelStats("Hex-A-Ring", LevelType.Survival, BestRecordType.Longest, false, true, 7, Properties.Resources.round_hex_a_ring_icon, Properties.Resources.round_hex_a_ring_big_icon) },
            { "round_hexsnake_almond",            new LevelStats("Hex-A-Terrestrial", LevelType.Survival, BestRecordType.Longest, false, true, 8, Properties.Resources.round_hex_a_terrestrial_icon, Properties.Resources.round_hex_a_terrestrial_big_icon) },
            { "round_jump_showdown",              new LevelStats("Jump Showdown", LevelType.Survival, BestRecordType.Longest, false, true, 1, Properties.Resources.round_jump_showdown_icon, Properties.Resources.round_jump_showdown_big_icon) },
            { "round_kraken_attack",              new LevelStats("Kraken Slam", LevelType.Survival, BestRecordType.Longest, false, true, 9, Properties.Resources.round_kraken_slam_icon, Properties.Resources.round_kraken_slam_big_icon) },
            { "round_crown_maze",                 new LevelStats("Lost Temple", LevelType.Race, BestRecordType.Fastest, false, true, 5, Properties.Resources.round_lost_temple_icon, Properties.Resources.round_lost_temple_big_icon) },
            { "round_tunnel_final",               new LevelStats("Roll Off", LevelType.Survival, BestRecordType.Longest, false, true, 3, Properties.Resources.round_roll_off_icon, Properties.Resources.round_roll_off_big_icon) },
            { "round_royal_rumble",               new LevelStats("Royal Fumble", LevelType.Hunt, BestRecordType.Fastest, false, true, 1, Properties.Resources.round_royal_fumble_icon, Properties.Resources.round_royal_fumble_big_icon) },
            { "round_thin_ice",                   new LevelStats("Thin Ice", LevelType.Survival, BestRecordType.Longest, false, true, 3, Properties.Resources.round_thin_ice_icon, Properties.Resources.round_thin_ice_big_icon) },
            { "round_tiptoefinale_almond",        new LevelStats("Tip Toe Finale", LevelType.Race, BestRecordType.Fastest, false, true, 8, Properties.Resources.round_tip_toe_finale_icon, Properties.Resources.round_tip_toe_finale_big_icon) },

            { "round_invisibeans",                new LevelStats("Sweet Thieves", LevelType.Invisibeans, BestRecordType.Fastest, false, true, 6, Properties.Resources.round_sweet_thieves_icon, Properties.Resources.round_sweet_thieves_big_icon) },

            //
            //S10 (SS4) Creative Rounds
            //
            { "wle_s10_orig_round_001",           new LevelStats("Beans Ahoy!", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "wle_s10_orig_round_002",           new LevelStats("Airborne Antics", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "wle_s10_orig_round_003",           new LevelStats("Scythes & Roundabouts", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "wle_s10_orig_round_004",           new LevelStats("Cardio Runners", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "wle_s10_orig_round_005",           new LevelStats("Fan Flingers", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "wle_s10_orig_round_006",           new LevelStats("Uphill Struggle", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "wle_s10_orig_round_007",           new LevelStats("Spinner Sprint", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "wle_s10_orig_round_008",           new LevelStats("Lane Changers", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "wle_s10_orig_round_009",           new LevelStats("Gentle Gauntlet", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "wle_s10_orig_round_012",           new LevelStats("Up & Down", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "wle_s10_orig_round_013",           new LevelStats("Choo Choo Challenge", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "wle_s10_orig_round_014",           new LevelStats("Runner Beans", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "wle_s10_orig_round_015",           new LevelStats("Disc Dashers", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "wle_s10_orig_round_016",           new LevelStats("Two Faced", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "wle_s10_orig_round_019",           new LevelStats("Blueberry Bombardment", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "wle_s10_orig_round_020",           new LevelStats("Chuting Stars", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "wle_s10_orig_round_021",           new LevelStats("Slimy Slopes", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "wle_s10_orig_round_022",           new LevelStats("Circuit Breakers", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "wle_s10_orig_round_023",           new LevelStats("Winding Walkways", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "wle_s10_orig_round_026",           new LevelStats("Hyperlink Hijinks", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "wle_s10_orig_round_027",           new LevelStats("Fan Frolics", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "wle_s10_orig_round_028",           new LevelStats("Windmill Road", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "wle_s10_orig_round_029",           new LevelStats("Conveyor Clash", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "wle_s10_orig_round_032",           new LevelStats("Fortress Frolics", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "wle_s10_orig_round_033",           new LevelStats("Super Door Dash", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "wle_s10_orig_round_034",           new LevelStats("Spiral Of Woo", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "wle_s10_orig_round_035",           new LevelStats("Tornado Trial", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "wle_s10_orig_round_036",           new LevelStats("Hopscotch Havoc", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "wle_s10_orig_round_037",           new LevelStats("Beat Bouncers", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "wle_s10_orig_round_038",           new LevelStats("Blunder Bridges", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "wle_s10_orig_round_039",           new LevelStats("Incline Rewind", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "wle_s10_orig_round_040",           new LevelStats("Prismatic Parade", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "wle_s10_orig_round_041",           new LevelStats("Swept Away", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "wle_s10_orig_round_042",           new LevelStats("Balancing Act", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "wle_s10_orig_round_043",           new LevelStats("Trouble Tower", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "wle_s10_orig_round_044",           new LevelStats("Serpent Slalom", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            // { "wle_s10_orig_round_045",           new LevelStats("Floorless", LevelType.Race, RecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "wle_s10_orig_round_046",           new LevelStats("In The Cloud", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "wle_s10_orig_round_047",           new LevelStats("Downstream Duel", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "wle_s10_orig_round_048",           new LevelStats("Lost Palace", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "wle_s10_orig_round_045_long",      new LevelStats("Floorless", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "wle_s10_long_round_003",           new LevelStats("Fall Speedway", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "wle_s10_long_round_004",           new LevelStats("Zig Zag Zoomies", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "wle_s10_long_round_005",           new LevelStats("Terrabyte Trial", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },

            { "wle_s10_round_001",                new LevelStats("Digi Trek", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "wle_s10_round_002",                new LevelStats("Shortcut Links", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "wle_s10_round_003",                new LevelStats("Upload Heights", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "wle_s10_round_005",                new LevelStats("Data Streams", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "wle_s10_round_006",                new LevelStats("Gigabyte Gauntlet", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "wle_s10_round_007",                new LevelStats("Cube Corruption", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "wle_s10_round_008",                new LevelStats("Wham Bam Boom", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "wle_s10_round_010",                new LevelStats("Pixel Hearts", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "wle_s10_round_011",                new LevelStats("Cyber Circuit", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "wle_s10_round_012",                new LevelStats("Boom Blaster Trial", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },

            { "wle_s10_player_round_wk3_01",      new LevelStats("Cloudy Chaos", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "wle_s10_player_round_wk3_02",      new LevelStats("Door Game", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "wle_s10_player_round_wk3_03",      new LevelStats("Full Speed Sliding (FSS) - Jelly Road", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "wle_s10_player_round_wk3_04",      new LevelStats("Sky High Run", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "wle_s10_player_round_wk3_06",      new LevelStats("Spiral Upheaval", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "wle_s10_player_round_wk3_07",      new LevelStats("Creative Descent", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "wle_s10_player_round_wk3_08",      new LevelStats("Rainbow Slide", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "wle_s10_player_round_wk3_09",      new LevelStats("Fragrant Trumpet", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "wle_s10_player_round_wk3_10",      new LevelStats("Bridges That Don't Like You", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "wle_s10_player_round_wk3_11",      new LevelStats("Rainbow Dash", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "wle_s10_player_round_wk3_12",      new LevelStats("Variable Valley", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "wle_fp2_wk6_01",                   new LevelStats("Broken Course", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "wle_s10_player_round_wk3_14",      new LevelStats("Tower of Fall", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "wle_s10_player_round_wk3_15",      new LevelStats("Parkour Party", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "wle_s10_player_round_wk3_16",      new LevelStats("Catastrophe Climb", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "wle_s10_player_round_wk3_17",      new LevelStats("Yeet Golf", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "wle_s10_player_round_wk3_18",      new LevelStats("Hill of Fear", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "wle_s10_player_round_wk3_19",      new LevelStats("Sky Time", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "wle_s10_player_round_wk3_20",      new LevelStats("Ezz Map", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },

            { "wle_s10_player_round_wk4_01",      new LevelStats("Slippery Stretch", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "wle_s10_player_round_wk4_02",      new LevelStats("Ball 'N Fall", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "wle_s10_player_round_wk4_03",      new LevelStats("Rowdy Cloudy", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "wle_s10_player_round_wk4_05",      new LevelStats("Vertiginous Steps", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "wle_s10_player_round_wk4_06",      new LevelStats("Topsie Tursie", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "wle_s10_player_round_wk4_07",      new LevelStats("Arcade Assault", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "wle_s10_player_round_wk4_08",      new LevelStats("The Eight Pit Trials", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "wle_s10_player_round_wk4_09",      new LevelStats("Green Beans", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "wle_s10_player_round_wk4_10",      new LevelStats("Hop Hill", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "wle_s10_player_round_wk4_11",      new LevelStats("Quick Sliders", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "wle_s10_player_round_wk4_12",      new LevelStats("Split Path", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "wle_s10_player_round_wk4_13",      new LevelStats("Piso Resbaloso", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "wle_s10_player_round_wk4_15",      new LevelStats("Snowboard Street", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "wle_s10_player_round_wk4_18",      new LevelStats("House Invasion", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "wle_s10_player_round_wk4_19",      new LevelStats("SOLO FULL-TILT RAGE", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "wle_s10_player_round_wk4_20",      new LevelStats("Terminal Slime-ocity", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "wle_s10_player_round_wk4_21",      new LevelStats("Spin", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "wle_s10_player_round_wk4_22",      new LevelStats("Lane Changers", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },

            { "wle_s10_player_round_wk5_01",      new LevelStats("Block Park", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "wle_s10_player_round_wk5_02",      new LevelStats("The Drummatical Story", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "wle_s10_player_round_wk5_03",      new LevelStats("Digital Temple", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "wle_s10_player_round_wk5_04",      new LevelStats("Tower Escape", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "wle_s10_player_round_wk5_05",      new LevelStats("Tower Dash", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "wle_s10_player_round_wk5_06",      new LevelStats("Gpu Gauntlet", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "wle_s10_player_round_wk5_07",      new LevelStats("Looooping", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "wle_s10_player_round_wk5_08",      new LevelStats("Rad Bean Skatepark", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "wle_s10_player_round_wk5_10",      new LevelStats("SiankArene", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "wle_s10_player_round_wk5_11",      new LevelStats("Pro Players Only", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "wle_s10_player_round_wk5_12",      new LevelStats("Extreme Tower", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "wle_s10_player_round_wk5_13",      new LevelStats("Dessert Village", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "wle_s10_player_round_wk5_14",      new LevelStats("Extreme Trampoline Jumping", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "wle_s10_player_round_wk5_15",      new LevelStats("Beast Route", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "wle_s10_player_round_wk5_16",      new LevelStats("METRÓPOLIS", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "wle_s10_player_round_wk5_17",      new LevelStats("Big Bookcase", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "wle_s10_player_round_wk5_18",      new LevelStats("Digital Doom", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },

            { "wle_s10_player_round_wk6_01",      new LevelStats("Hammer Heaven", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "wle_s10_player_round_wk6_02",      new LevelStats("RISKY ROUTES", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "wle_s10_player_round_wk6_03",      new LevelStats("Castle Rush", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "wle_s10_player_round_wk6_04",      new LevelStats("Chaotic Race", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "wle_s10_player_round_wk6_05",      new LevelStats("FREEFALL TOWER", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "wle_s10_player_round_wk6_06",      new LevelStats("西西的天空之城 Castle in the Sky", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "wle_s10_player_round_wk6_08",      new LevelStats("Flower Power", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "wle_s10_player_round_wk6_09",      new LevelStats("Dimension Explorer", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "wle_s10_player_round_wk6_10",      new LevelStats("Forked Passage", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "wle_s10_player_round_wk6_12",      new LevelStats("The Bee Hive", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "wle_s10_player_round_wk6_13",      new LevelStats("Yeets & Ladders", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "wle_s10_player_round_wk6_14",      new LevelStats("Snek", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "wle_s10_player_round_wk6_15",      new LevelStats("SCHOOL OF FISH", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "wle_s10_player_round_wk6_17",      new LevelStats("Slippery Helixes", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "wle_s10_player_round_wk6_18",      new LevelStats("Recess", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "wle_s10_player_round_wk6_19",      new LevelStats("Parrot river", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "wle_s10_player_round_wk6_20",      new LevelStats("PARKOUR CITY 城市跑酷", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },

            { "current_wle_fp3_07_01",            new LevelStats("Block Sledding", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp3_07_02",            new LevelStats("Layup Wallop", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp3_07_03",            new LevelStats("Minecart Mayhem", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp3_07_04",            new LevelStats("Bouncing Pass", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp3_07_05",            new LevelStats("Ball Factory", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp3_07_0_01",          new LevelStats("Funky Sanctuaries", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp3_07_0_02",          new LevelStats("Woo-F-O", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp3_07_0_03",          new LevelStats("Travel Diary - Great Wall of China", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },

            { "current_wle_fp3_08_01",            new LevelStats("Grabbers Territory", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp3_08_02",            new LevelStats("A Way Out", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp3_08_03",            new LevelStats("Wall Block", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp3_08_04",            new LevelStats("The dream island", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp3_08_05",            new LevelStats("Rainbow pulsion", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp3_08_06",            new LevelStats("WHIPPITY WOPPITY", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp3_08_09",            new LevelStats("Big Fans Box Challenge", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp3_08_10",            new LevelStats("Crazy boxes", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp3_08_13",            new LevelStats("Season 1 Race Mashup", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp3_08_14",            new LevelStats("Flippy Hoopshots", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp3_08_15",            new LevelStats("Stumble Teams", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp3_08_16",            new LevelStats("Twisting Tower", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp3_08_17",            new LevelStats("PUSH 'N' PULL", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp3_08_18",            new LevelStats("The Rising Blocks", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp3_08_19",            new LevelStats("Puzzle Blokies Path", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },

            { "current_wle_fp3_09_01",            new LevelStats("The up tower", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp3_09_02",            new LevelStats("Short shuriken", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp3_09_03",            new LevelStats("Les mêmes mécaniques de + en + dure", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp3_09_04",            new LevelStats("Digi-Lily Sliding", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp3_09_05",            new LevelStats("STUMBLE MEDIEVAL TOWER", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp3_09_06",            new LevelStats("Random Heights", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp3_09_07",            new LevelStats("Climb scramble", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp3_09_08",            new LevelStats("Collide Gaming", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp3_09_09",            new LevelStats("Very Compressed Level", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp3_09_0_01",          new LevelStats("Slippery Slope", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp3_09_0_02",          new LevelStats("The Most Hardest Fall Guys LEVEL", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp3_09_0_03",          new LevelStats("Free Falling", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp3_09_0_04",          new LevelStats("Conveyor Problems", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp3_09_0_05",          new LevelStats("Clocktower Climb", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp3_09_0_06",          new LevelStats("Savour Your Happiness", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp3_09_0_0_01",        new LevelStats("Pastel Paradise", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },

            { "current_wle_fp3_10_01",            new LevelStats("When Nature Falls", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp3_10_02",            new LevelStats("The Slippery Conveyor", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp3_10_03",            new LevelStats("The Slime Trials", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp3_10_04",            new LevelStats("Friendly Obstacles", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp3_10_05",            new LevelStats("Climb and Fall", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp3_10_06",            new LevelStats("Stairs and some other things", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp3_10_07",            new LevelStats("Meowgical World", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp3_10_08",            new LevelStats("Polluelo Speed", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp3_10_09",            new LevelStats("Pixel Parade", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp3_10_10",            new LevelStats("Total Madness", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp3_10_11",            new LevelStats("The Abstract Maze", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp3_10_12",            new LevelStats("Fan Off", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp3_10_13",            new LevelStats("cloud highway", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp3_10_14",            new LevelStats("はねるの！？トビラ（Door Bounce）", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp3_10_15",            new LevelStats("Speedrunners be like", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp3_10_16",            new LevelStats("Tumble Tower", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp3_10_17",            new LevelStats("Silver's Snake Run", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp3_10_18",            new LevelStats("Now Boarding", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp3_10_19",            new LevelStats("Slime Scale", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp3_10_20",            new LevelStats("TUMBLEDOWN MINESHAFT", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp3_10_21",            new LevelStats("Circuito CHILL 1", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp3_10_22",            new LevelStats("STUMBLE SLIDER", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp3_10_23",            new LevelStats("Controlled Chaos", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp3_10_24",            new LevelStats("Xtreme Jumping", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp3_10_25",            new LevelStats("Odin", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp3_10_26",            new LevelStats("Ciudad nube", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp3_10_27",            new LevelStats("Bean Voyage", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp3_10_28",            new LevelStats("SLIP-SAW", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp3_10_29",            new LevelStats("Bbq bacon burger", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },

            { "current_wle_fp4_05_01_01",         new LevelStats("巅峰车站", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp4_05_01_02",         new LevelStats("Beba Park 7", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp4_05_01_03",         new LevelStats("Rainbow in the clouds", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp4_05_01_04",         new LevelStats("So You Think You Can Bounce", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp4_05_01_05",         new LevelStats("Bounce Back Racetrack", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp4_05_01",            new LevelStats("¡!OLYMPE¡!", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp4_05_02",            new LevelStats("Mini only up", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp4_05_03_01",         new LevelStats("Cubic Conundrum", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp4_05_03_02",         new LevelStats("Remote Control", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp4_05_03",            new LevelStats("Parkway Slide", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp4_05_04",            new LevelStats("Duel Dash", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp4_05_05",            new LevelStats("beaten trackless road", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp4_05_2_01",          new LevelStats("¡River Paradise!", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp4_05_2_02",          new LevelStats("arch city", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp4_05_2_03",          new LevelStats("Desert Ruins", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },

            { "current_wle_fp4_06_01",            new LevelStats("PENTAGON CIRCUIT", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp4_06_02",            new LevelStats("Pachislo", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp4_06_0_01",          new LevelStats("AquArsene", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp4_06_0_02",          new LevelStats("RainbowCloud", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp4_06_0_03",          new LevelStats("Pink Cascade", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp4_06_0_04",          new LevelStats("Conveyor Conundrum", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp4_06_0_05",          new LevelStats("RICKETY STRAWBRIDGE", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp4_06_0_10_01",       new LevelStats("The Bee Hive", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp4_06_1_01",          new LevelStats("Buggin' Out", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp4_06_1_02",          new LevelStats("RISE AND SLIDE", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp4_06_1_03",          new LevelStats("Bean Mini Golf", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp4_06_1_04",          new LevelStats("Youpii Youpii", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp4_06_1_05",          new LevelStats("The climb of Trials", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp4_06_1_06",          new LevelStats("Bouncy Castle", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },

            { "current_wle_fp4_07_01",            new LevelStats("Rotational Runner", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp4_07_02",            new LevelStats("SPIRAL DASH ROAD", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp4_07_03",            new LevelStats("simple stage", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp4_07_04",            new LevelStats("Slip Slide Jump and Run", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp4_07_05",            new LevelStats("Factory Valley", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp4_07_06",            new LevelStats("Jumpy Beans", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp4_07_07",            new LevelStats("Slimetastic Stumble", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp4_07_0_01",          new LevelStats("Camino Ninja", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },

            { "current_wle_fp4_08_01",            new LevelStats("co-op guys", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp4_08_0_01",          new LevelStats("The big slide", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp4_08_0_02",          new LevelStats("Freefall Mountain", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp4_08_0_03",          new LevelStats("Hazy Stairways", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp4_08_0_04",          new LevelStats("Pillar Promenade", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp4_08_0_05",          new LevelStats("Hidden Treasure of Magical Castle", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp4_08_0_06",          new LevelStats("X-Course", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp4_08_0_07",          new LevelStats("Speed Gauntlet", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp4_08_1_01",          new LevelStats("Boost in Dash", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp4_08_1_02",          new LevelStats("Rainbow Raceway", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp4_08_1_03",          new LevelStats("Giddy up!", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp4_08_1_04",          new LevelStats("Mad lab", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp4_08_2_01",          new LevelStats("Convoluted Conveyors", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp4_08_3_01",          new LevelStats("The Oasis", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },

            { "current_wle_fp4_09_01",            new LevelStats("Crate Collector", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp4_09_02",            new LevelStats("Dribble Drills", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp4_09_03",            new LevelStats("Spinning Slide Dodge", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp4_09_04",            new LevelStats("Skyline Park", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp4_09_05",            new LevelStats("Birthday bonanza", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp4_09_06",            new LevelStats("The Chaotic Waterfall", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp4_09_0_01",          new LevelStats("ICY PEAKS", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp4_09_1_01",          new LevelStats("Push-Box Chaos", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp4_09_1_02",          new LevelStats("Haute voltige", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp4_09_2_01",          new LevelStats("DNA Test", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },

            { "current_wle_fp4_10_01",            new LevelStats("Bouncy Box Boulevard 3 Extreme Delivery", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp4_10_02",            new LevelStats("Hot Blast", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp4_10_03",            new LevelStats("Box Fan Blitz", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp4_10_04",            new LevelStats("Woo-terfall Way", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp4_10_05",            new LevelStats("Slime race", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp4_10_06",            new LevelStats("Moving Day", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp4_10_07",            new LevelStats("Birthday Dash", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp4_10_08_m",          new LevelStats("Wall Breaker", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp4_10_08",            new LevelStats("Chess History", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp4_10_11",            new LevelStats("HOARDER BLOCKS", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp4_10_12",            new LevelStats("Chickens run away", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp4_10_20",            new LevelStats("Co-op and CO", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp4_10_21",            new LevelStats("Construction Site", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp4_10_0_01",          new LevelStats("Cheese Canyon", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp4_10_0_02",          new LevelStats("Molehills", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },

            { "current_wle_fp5_2_01_01",          new LevelStats("Plummet Summit", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp5_2_01_02",          new LevelStats("タワークライム", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp5_2_01_03",          new LevelStats("けいけいどうくつ", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp5_2_01_04",          new LevelStats("Quicksand Valley 01", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp5_2_01",             new LevelStats("Jabonsotes", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp5_2_02_01",          new LevelStats("のっぽタワー", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp5_2_02_02",          new LevelStats("Blaster Course", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp5_2_02_03",          new LevelStats("Panna Cotta Guys", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp5_2_02_04",          new LevelStats("La subida Hard", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp5_2_02_05",          new LevelStats("sliding guys", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp5_2_02_06",          new LevelStats("Hallow's Haunt", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp5_2_02",             new LevelStats("Rainbow highway", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp5_2_03",             new LevelStats("Falligator", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp5_2_04_01",          new LevelStats("Lily Lovers", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp5_2_04_02",          new LevelStats("rainow rode", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp5_2_04_03",          new LevelStats("L' Rumorosa", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp5_2_04_04",          new LevelStats("Super crazy level", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp5_2_04",             new LevelStats("Big WaterFall Planet（巨大な滝の惑星）", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp5_2_05_01",          new LevelStats("Super crazy level", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp5_2_05_02",          new LevelStats("Lily Lovers", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp5_2_05_03",          new LevelStats("L' Rumorosa", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp5_2_05_04",          new LevelStats("rainbow roade", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp5_2_05",             new LevelStats("The tension Parkour", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp5_2_06",             new LevelStats("Stairing Contest", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp5_2_07",             new LevelStats("Vibrant Ascension", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },

            { "current_wle_fp5_3_05_01",          new LevelStats("Beany Golf!", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp5_3_05_02_01",       new LevelStats("Speedy Bros", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp5_4_01_01",          new LevelStats("Tilted Walls", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp5_4_01_02",          new LevelStats("The Lost Diamond", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp5_4_01_03",          new LevelStats("The Flowery Garden", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp5_4_01_04",          new LevelStats("Happy Bean Cube Land 2", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp5_4_01_05",          new LevelStats("Traffic Jamboree", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },

            { "current_wle_fp5_10_01",            new LevelStats("Breezy Buddies", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp5_10_0_01",          new LevelStats("Salto Plancha", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp5_10_0_02",          new LevelStats("My house", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp5_10_0_03",          new LevelStats("Sleepwalk Speedway", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp5_10_1_01",          new LevelStats("Desert Ruins", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp5_10_1_02",          new LevelStats("¡River Paradise!", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp5_10_2_01",          new LevelStats("Desert Ruins", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp5_10_2_02",          new LevelStats("¡River Paradise!", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp5_10_2_03",          new LevelStats("Mystic River", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp5_10_2_04",          new LevelStats("Split Path Stumble", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp5_10_2_05",          new LevelStats("Cliffside", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp5_10_2_06",          new LevelStats("Tricky Turnpike", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp5_10_2_07",          new LevelStats("The Avenue", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp5_10_2_08",          new LevelStats("Gold City", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp5_10_2_09",          new LevelStats("タイトル決まりませんｗ", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp5_10_2_10",          new LevelStats("初心者用コース", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp5_10_2_11",          new LevelStats("Chain Reaction", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },

            { "current_wle_fp5_wk3_1_01",         new LevelStats("Gobille infernal", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp5_wk3_1_02",         new LevelStats("SLIMY RACE", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp5_wk3_1_03",         new LevelStats("Bouncy Bros", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp5_wk3_1_04",         new LevelStats("El airecito", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp5_wk3_1_05",         new LevelStats("Crown Tunnel", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp5_wk3_1_06",         new LevelStats("Sweet Sweet Scurry", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp5_wk3_2_01",         new LevelStats("Magical Journey", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp5_wk3_2_02",         new LevelStats("The Crown Jewel", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp5_wk3_2_03",         new LevelStats("From bottom to top", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp5_wk3_2_04",         new LevelStats("The Throne Of The Best King", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp5_wk3_2_05",         new LevelStats("Honeycomb Highway", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp5_wk3_3_01",         new LevelStats("Bouncier Heights", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp5_wk3_3_02",         new LevelStats("Rainbow Racers", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp5_wk3_3_03",         new LevelStats("Bye have a great time part 24", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp5_wk3_3_04",         new LevelStats("speedrun snek", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },

            { "current_wle_fp5_falloween_1_01",    new LevelStats("Falloween Dash", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp5_falloween_1_02",    new LevelStats("深秋寻路 Fall Routes", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp5_falloween_1_03",    new LevelStats("Синни Санни Сонни", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp5_falloween_1_04",    new LevelStats("Falloween Festival!", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp5_falloween_1_05",    new LevelStats("Hearth Attack", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp5_falloween_1_06",    new LevelStats("Mirage", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp5_falloween_1_07",    new LevelStats("Spooky Sprinter", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp5_falloween_1_08",    new LevelStats("Toon World", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp5_falloween_1_09",    new LevelStats("Creepy climb", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp5_falloween_1_10",    new LevelStats("Spooky Buttons", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp5_falloween_1_11",    new LevelStats("Spider's Lair", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp5_falloween_1_12",    new LevelStats("Night of goosebumps - Trick or Treat", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp5_falloween_1_13",    new LevelStats("The Falloween Spirits", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp5_falloween_1_14",    new LevelStats("Jack-O'-Runners", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp5_falloween_2_01",    new LevelStats("spooky nightwalk", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp5_falloween_2_02",    new LevelStats("Creepy Crawly Canyon", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp5_falloween_2_03_01", new LevelStats("Spider Web Escape", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp5_falloween_2_03_02", new LevelStats("Fallen Beans", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp5_falloween_2_03_03", new LevelStats("Pumpkin Patch Dash", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp5_falloween_2_03_04", new LevelStats("Going batty", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp5_falloween_2_03_05", new LevelStats("Bone Bridges", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp5_falloween_2_03_06", new LevelStats("Spider", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp5_falloween_2_03",    new LevelStats("fastoween", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp5_falloween_3_01",    new LevelStats("Haunted Hoops", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp5_falloween_3_02",    new LevelStats("SPIDER SCURRY", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp5_falloween_3_03",    new LevelStats("Fallobeans", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp5_falloween_3_04",    new LevelStats("Escape the haunted mansion", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp5_falloween_4_01",    new LevelStats("Cursed Catacombs", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp5_falloween_4_02",    new LevelStats("Altar de Alubias", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp5_falloween_4_03",    new LevelStats("This is Fall O Ween", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp5_falloween_4_04",    new LevelStats("Belly of the Beast", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp5_falloween_4_05",    new LevelStats("Scary Final", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp5_falloween_4_06",    new LevelStats("Slip 'N' Scare", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp5_falloween_4_07",    new LevelStats("Spooky Speedy Arches", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp5_falloween_4_08",    new LevelStats("Catoween", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp5_falloween_4_09",    new LevelStats("Jack-O'-Zoom", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp5_falloween_4_10",    new LevelStats("Bean´s Spooky Route", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp5_falloween_4_11",    new LevelStats("Tricks and treats", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp5_falloween_4_12",    new LevelStats("HAUNTED CAT MANSION", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp5_falloween_4_13",    new LevelStats("Feeling Batty", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp5_falloween_4_14",    new LevelStats("Monster Dash", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp5_falloween_4_15",    new LevelStats("Pumpkin Parkour", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp5_falloween_4_16",    new LevelStats("Cauldron of Treats", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp5_falloween_4_17",    new LevelStats("Eerie Enigma", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp5_falloween_4_18",    new LevelStats("Candy Cruisers", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp5_falloween_4_19",    new LevelStats("Pumpkin eater", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp5_falloween_5_01",    new LevelStats("Spooky Run", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp5_falloween_5_02",    new LevelStats("HORROR NIGHT RITUAL", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp5_falloween_5_03",    new LevelStats("Spooky slime race", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp5_falloween_5_04",    new LevelStats("Halloween", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp5_falloween_5_05",    new LevelStats("Spooky Momentum", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp5_falloween_5_06",    new LevelStats("Cloud 9", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp5_falloween_5_07",    new LevelStats("HAUNTED HAUL", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp5_falloween_6_01",    new LevelStats("Coffin Droppin'", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp5_falloween_6_02",    new LevelStats("Tricky Treats!", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp5_falloween_6_03",    new LevelStats("Happy Falloween", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp5_falloween_7_01_01", new LevelStats("The Haunted Castle", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp5_falloween_7_01_02", new LevelStats("City of fear", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp5_falloween_7_01_03", new LevelStats("Monster Mayhem", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp5_falloween_7_01_04", new LevelStats("Beyond The Grave Rave", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp5_falloween_7_01_05", new LevelStats("Beyond The Grave Rave", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp5_falloween_7_01_06", new LevelStats("Falloween Monster", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp5_falloween_7_01_07", new LevelStats("Falloween Castle", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp5_falloween_7_01_08", new LevelStats("Halloween sky course", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp5_falloween_7_01_09", new LevelStats("WICKED RACE", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp5_falloween_7_02_01", new LevelStats("Happy Bean Spooky Tumble", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp5_falloween_7_03_01", new LevelStats("Horror Movie Night", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp5_falloween_7_04_01", new LevelStats("Night Temptation（ナイトテンプテーション）", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp5_falloween_7_04_02", new LevelStats("Fall Haunted", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp5_falloween_7_04_03", new LevelStats("Color Climb", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp5_falloween_7_04_04", new LevelStats("Pumpkin (Speed) Party", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp5_falloween_7_05_01", new LevelStats("FALLOWEEN FOREST TUMBLE", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp5_falloween_7_05_02", new LevelStats("Ominous Outpost", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp5_falloween_7_05_03", new LevelStats("Spook Circuit", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp5_falloween_7_05_04", new LevelStats("Path of horrors", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp5_falloween_7_06_01", new LevelStats("PUMPKIN KINGDOM", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp5_falloween_7_06_02", new LevelStats("Grave Danger Graveyard", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp5_falloween_7_06_03", new LevelStats("Coffin Droppin'", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp5_falloween_7_06_04", new LevelStats("Skeleton Climb", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp5_falloween_7_06_05", new LevelStats("Falloween Heights", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp5_falloween_7_08_01", new LevelStats("Trick ! - Falloween party", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp5_falloween_7_10_01", new LevelStats("NEW OBSTACLES! - The Falloween Trial", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp5_falloween_7_10_02", new LevelStats("Nightmare Mansion", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp5_falloween_9_01",    new LevelStats("Skeleton Mayhem", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp5_falloween_9_02",    new LevelStats("Raveyard", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp5_falloween_9_03",    new LevelStats("Spooky Sprint", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp5_falloween_9_04",    new LevelStats("Collapsed World 2", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp5_falloween_9_05",    new LevelStats("Maximum Turbulance", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp5_falloween_10_01",   new LevelStats("Graveyard Mansion", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp5_falloween_10_02",   new LevelStats("Trick Treat Fall", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp5_falloween_11_01",   new LevelStats("Night Runners", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp5_falloween_12_01",   new LevelStats("Haunted Mansion", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp5_falloween_12_02",   new LevelStats("Spooky Orange and Purple", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp5_falloween_13_01",   new LevelStats("Witch Way", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp5_falloween_14_01",   new LevelStats("Nightmare Bump", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp5_falloween_15_01",   new LevelStats("Spooktacular Fun Run", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },

            { "current_wle_fp6_1_01",             new LevelStats("Boo-mblaster Betrayal", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp6_1_02",             new LevelStats("MOUNT SMOODGIE!", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp6_1_03",             new LevelStats("SKULL KING", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp6_1_04",             new LevelStats("shorter circuit", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp6_1_05",             new LevelStats("be speedy", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp6_1_06",             new LevelStats("Sky-City Stumble", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp6_1_07",             new LevelStats("Wormhole Mountain", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp6_1_08",             new LevelStats("Sweet Tooth Slide", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp6_1_09",             new LevelStats("Splitspeed", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp6_1_10",             new LevelStats("Fall Canyon", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp6_2_01",             new LevelStats("Valise", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp6_2_02",             new LevelStats("Big Yeetus challenge 2 FaIIoween", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp6_2_03",             new LevelStats("Water Way", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp6_2_04",             new LevelStats("The Doors of Doom", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp6_2_05",             new LevelStats("SPOOKY CEMETERY", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp6_2_06",             new LevelStats("Liana Swings", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp6_2_07",             new LevelStats("Colourful", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp6_2_08",             new LevelStats("The Heist!", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp6_2_09",             new LevelStats("Factory of Fallmecanic", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp6_3_01",             new LevelStats("Tricky Treat", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp6_3_02",             new LevelStats("The Lighthouse", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp6_3_03",             new LevelStats("どっちがかんたんだとおもいますか？", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp6_3_04",             new LevelStats("The Scarescraper", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp6_3_05",             new LevelStats("Obstacle Road", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp6_3_06",             new LevelStats("Rest In Paris", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp6_3_07",             new LevelStats("Horror pumpkim", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "current_wle_fp6_3_08",             new LevelStats("时间停止了(Time Stop)", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },

            { "wle_fp5_falloween_round_1",        new LevelStats("The Haunted Castle", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "wle_fp5_falloween_round_2",        new LevelStats("City of fear", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "wle_fp5_falloween_round_3",        new LevelStats("Happy Bean Spooky Tumble", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "wle_fp5_falloween_round_4",        new LevelStats("Monster Mayhem", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "wle_fp5_falloween_round_5",        new LevelStats("Beyond The Grave Rave", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "wle_fp5_falloween_round_6",        new LevelStats("Falloween Monster", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "wle_fp5_falloween_round_7",        new LevelStats("Falloween Castle", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "wle_fp5_falloween_round_8",        new LevelStats("Halloween sky course", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "wle_fp5_falloween_round_9",        new LevelStats("WICKED RACE", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "wle_fp5_falloween_round_10",       new LevelStats("Night Temptation（ナイトテンプテーション）", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "wle_fp5_falloween_round_12",       new LevelStats("NEW OBSTACLES! - The Falloween Trial", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "wle_fp5_falloween_round_13",       new LevelStats("Color Climb", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "wle_fp5_falloween_round_14",       new LevelStats("Pumpkin (Speed) Party", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "wle_fp5_falloween_round_15",       new LevelStats("Horror Movie Night", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "wle_fp5_falloween_round_16",       new LevelStats("FALLOWEEN FOREST TUMBLE", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "wle_fp5_falloween_round_17",       new LevelStats("PUMPKIN KINGDOM", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "wle_fp5_falloween_round_18",       new LevelStats("Ominous Outpost", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "wle_fp5_falloween_round_19",       new LevelStats("Spook Circuit", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "wle_fp5_falloween_round_20",       new LevelStats("Grave Danger Graveyard", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "wle_fp5_falloween_round_22",       new LevelStats("Skeleton Climb", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "wle_fp5_falloween_round_23",       new LevelStats("Falloween Heights", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "wle_fp5_falloween_round_25",       new LevelStats("Falloween Festival!", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "wle_fp5_falloween_round_26",       new LevelStats("Night of goosebumps - Trick or Treat", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "wle_fp5_falloween_round_27",       new LevelStats("Jack-O'-Runners", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "wle_fp5_falloween_round_29",       new LevelStats("Spider's Lair", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "wle_fp5_falloween_round_30",       new LevelStats("Falloween Dash", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "wle_fp5_falloween_round_31",       new LevelStats("Hearth Attack", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "wle_fp5_falloween_round_32",       new LevelStats("Mirage", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "wle_fp5_falloween_round_33",       new LevelStats("spooky nightwalk", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "wle_fp5_falloween_round_34",       new LevelStats("Tricky Treats!", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "wle_fp5_falloween_round_35",       new LevelStats("Happy Falloween", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "wle_fp5_falloween_round_36",       new LevelStats("Night Runners", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "wle_fp5_falloween_round_37",       new LevelStats("Creepy Crawly Canyon", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "wle_fp5_falloween_round_38",       new LevelStats("Creepy climb", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "wle_fp5_falloween_round_40",       new LevelStats("Toon World", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "wle_fp5_falloween_round_41",       new LevelStats("fastoween", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "wle_fp5_falloween_round_42",       new LevelStats("This is Fall O Ween", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "wle_fp5_falloween_round_43",       new LevelStats("Tricks and treats", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "wle_fp5_falloween_round_44",       new LevelStats("Catoween", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "wle_fp5_falloween_round_45",       new LevelStats("Feeling Batty", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "wle_fp5_falloween_round_47",       new LevelStats("HAUNTED CAT MANSION", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "wle_fp5_falloween_round_48",       new LevelStats("Skeleton Mayhem", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "wle_fp5_falloween_round_49",       new LevelStats("Spider Web Escape", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "wle_fp5_falloween_round_50",       new LevelStats("Spooky Sprint", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "wle_fp5_falloween_round_51",       new LevelStats("Raveyard", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "wle_fp5_falloween_round_52",       new LevelStats("Collapsed World 2", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "wle_fp5_falloween_round_53",       new LevelStats("Fallen Beans", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "wle_fp5_falloween_round_54",       new LevelStats("Maximum Turbulance", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "wle_fp5_falloween_round_55",       new LevelStats("Pumpkin Patch Dash", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "wle_fp5_falloween_round_56",       new LevelStats("Going batty", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "wle_fp5_falloween_round_57",       new LevelStats("Bone Bridges", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "wle_fp5_falloween_round_58",       new LevelStats("Spider", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "wle_fp5_falloween_round_59",       new LevelStats("Graveyard Mansion", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "wle_fp5_falloween_round_60",       new LevelStats("Nightmare Bump", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "wle_fp5_falloween_round_61",       new LevelStats("Trick Treat Fall", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "wle_fp5_falloween_round_64",       new LevelStats("Haunted Mansion", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "wle_fp5_falloween_round_65",       new LevelStats("Spooky Orange and Purple", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "wle_fp5_falloween_round_66",       new LevelStats("Witch Way", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "wle_fp5_falloween_round_67",       new LevelStats("Altar de Alubias", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "wle_fp5_falloween_round_68",       new LevelStats("Jack-O'-Zoom", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "wle_fp5_falloween_round_69",       new LevelStats("Pumpkin eater", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "wle_fp5_falloween_round_70",       new LevelStats("Spooky Momentum", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "wle_fp5_falloween_round_71",       new LevelStats("Halloween", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "wle_fp5_falloween_round_72",       new LevelStats("Spooky slime race", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "wle_fp5_falloween_round_73",       new LevelStats("HAUNTED HAUL", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "wle_fp5_falloween_round_74",       new LevelStats("Cloud 9", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "wle_fp5_falloween_round_75",       new LevelStats("Bean´s Spooky Route", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "wle_fp5_falloween_round_76",       new LevelStats("Scary Final", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "wle_fp5_falloween_round_77",       new LevelStats("Pumpkin Parkour", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "wle_fp5_falloween_round_78",       new LevelStats("Slip 'N' Scare", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "wle_fp5_falloween_round_79",       new LevelStats("Eerie Enigma", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "wle_fp5_falloween_round_80",       new LevelStats("Spooky Speedy Arches", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "wle_fp5_falloween_round_81",       new LevelStats("Monster Dash", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "wle_fp5_falloween_round_82",       new LevelStats("Cursed Catacombs", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "wle_fp5_falloween_round_83",       new LevelStats("Cauldron of Treats", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "wle_fp5_falloween_round_84",       new LevelStats("Belly of the Beast", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "wle_fp5_falloween_round_85",       new LevelStats("Candy Cruisers", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "wle_fp5_falloween_round_86",       new LevelStats("Синни Санни Сонни", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "wle_fp5_falloween_round_87",       new LevelStats("Escape the haunted mansion", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "wle_fp5_falloween_round_88",       new LevelStats("Fallobeans", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "wle_fp5_falloween_round_90",       new LevelStats("SPIDER SCURRY", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },

            { "wle_s10_bt_round_001",             new LevelStats("Push Ups", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "wle_s10_bt_round_002",             new LevelStats("Heave & Haul", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "wle_s10_bt_round_003",             new LevelStats("Stepping Stones", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "wle_s10_bt_round_004",             new LevelStats("Double Trouble", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },

            { "wle_s10_cf_round_001",             new LevelStats("Blocky Bridges", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "wle_s10_cf_round_002",             new LevelStats("Gappy-go-Lucky", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "wle_s10_cf_round_003",             new LevelStats("Drop n' Drag", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "wle_s10_cf_round_004",             new LevelStats("Fun with Fans", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },

            { "wle_mrs_bagel_opener_1",           new LevelStats("Tunnel of Love", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "wle_mrs_bagel_opener_2",           new LevelStats("Pink Parade", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "wle_mrs_bagel_opener_3",           new LevelStats("Prideful Path", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "wle_mrs_bagel_opener_4",           new LevelStats("Coming Together", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "wle_mrs_bagel_filler_1",           new LevelStats("Clifftop Capers", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "wle_mrs_bagel_filler_2",           new LevelStats("Waveway Splits", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "wle_mrs_bagel_filler_3",           new LevelStats("In the Groove", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "wle_mrs_bagel_filler_4",           new LevelStats("Heartfall Heat", LevelType.Race, BestRecordType.Fastest, true, false, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "wle_mrs_bagel_final_1",            new LevelStats("Rainbow Rise", LevelType.Race, BestRecordType.Fastest, true, true, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "wle_mrs_bagel_final_2",            new LevelStats("Out and About", LevelType.Race, BestRecordType.Fastest, true, true, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },

            { "wle_s10_orig_round_010",           new LevelStats("Square Up", LevelType.Race, BestRecordType.Fastest, true, true, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "wle_s10_orig_round_011",           new LevelStats("Slide Showdown", LevelType.Race, BestRecordType.Fastest, true, true, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "wle_s10_orig_round_017",           new LevelStats("Bellyflop Battlers", LevelType.Race, BestRecordType.Fastest, true, true, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "wle_s10_orig_round_018",           new LevelStats("Apples & Oranges", LevelType.Race, BestRecordType.Fastest, true, true, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "wle_s10_orig_round_024",           new LevelStats("Wooseleum", LevelType.Race, BestRecordType.Fastest, true, true, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "wle_s10_orig_round_025",           new LevelStats("Mount Boom", LevelType.Race, BestRecordType.Fastest, true, true, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "wle_s10_orig_round_030",           new LevelStats("Mega Monument", LevelType.Race, BestRecordType.Fastest, true, true, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "wle_s10_orig_round_031",           new LevelStats("Transfer Turnpike", LevelType.Race, BestRecordType.Fastest, true, true, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "wle_s10_round_004",                new LevelStats("Parkour Panic", LevelType.Race, BestRecordType.Fastest, true, true, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
            { "wle_s10_round_009",                new LevelStats("Firewall Finale", LevelType.Race, BestRecordType.Fastest, true, true, 10, Properties.Resources.round_gauntlet_icon, Properties.Resources.round_gauntlet_big_icon) },
        };
        public static Dictionary<string, string> SceneToRound = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) {
            { "FallGuy_DoorDash",                  "round_door_dash" },
            { "FallGuy_Gauntlet_02_01",            "round_gauntlet_02" },
            { "FallGuy_DodgeFall",                 "round_dodge_fall" },
            { "FallGuy_ChompChomp_01",             "round_chompchomp" },
            { "FallGuy_Gauntlet_01",               "round_gauntlet_01" },
            { "FallGuy_SeeSaw_variant2",           "round_see_saw" },
            { "FallGuy_Lava_02",                   "round_lava" },
            { "FallGuy_TipToe",                    "round_tip_toe" },
            { "FallGuy_Gauntlet_03",               "round_gauntlet_03" },
            { "FallGuy_Block_Party",               "round_block_party" },
            { "FallGuy_JumpClub_01",               "round_jump_club" },
            { "FallGuy_MatchFall",                 "round_match_fall" },
            { "FallGuy_Tunnel_01",                 "round_tunnel" },
            { "FallGuy_TailTag_2",                 "round_tail_tag" },
            { "FallGuy_EggGrab",                   "round_egg_grab" },
            { "FallGuy_FallBall_5",                "round_fall_ball_60_players" },
            { "FallGuy_BallHogs_01",               "round_ballhogs" },
            { "FallGuy_Hoops_01",                  "round_hoops" },
            { "FallGuy_TeamInfected",              "round_jinxed" },
            { "FallGuy_RocknRoll",                 "round_rocknroll" },
            { "FallGuy_ConveyorArena_01",          "round_conveyor_arena" },
            { "FallGuy_FallMountain_Hub_Complete", "round_fall_mountain_hub_complete" },
            { "FallGuy_FloorFall",                 "round_floor_fall" },
            { "FallGuy_JumpShowdown_01",           "round_jump_showdown" },
            { "FallGuy_Arena_01",                  "round_royal_rumble" },

            { "FallGuy_BiggestFan",                "round_biggestfan" },
            { "FallGuy_Hoops_Blockade",            "round_hoops_blockade_solo" },
            { "FallGuy_Gauntlet_04",               "round_gauntlet_04" },
            { "FallGuy_WallGuys",                  "round_wall_guys" },
            { "FallGuy_EggGrab_02",                "round_egg_grab_02" },

            { "FallGuy_IceClimb_01",               "round_iceclimb" },
            { "FallGuy_SkeeFall",                  "round_skeefall" },
            { "FallGuy_Gauntlet_05",               "round_gauntlet_05" },
            { "FallGuy_SnowballSurvival",          "round_snowballsurvival" },
            { "FallGuy_ChickenChase_01",           "round_chicken_chase" },
            { "FallGuy_Snowy_Scrap",               "round_snowy_scrap" },
            { "FallGuy_Tunnel_Final",              "round_tunnel_final" },
            { "FallGuy_ThinIce",                   "round_thin_ice" },

            { "FallGuy_1v1_ButtonBasher",          "round_1v1_button_basher" },
            { "FallGuy_Tunnel_Race_01",            "round_tunnel_race" },
            { "FallGuy_ShortCircuit",              "round_shortcircuit" },
            { "FallGuy_Gauntlet_06",               "round_gauntlet_06" },
            { "FallGuy_SlimeClimb_2",              "round_slimeclimb_2" },
            { "FallGuy_FruitPunch",                "round_fruitpunch_s4_show" },
            { "FallGuy_HoverboardSurvival",        "round_hoverboardsurvival_s4_show" },
            { "FallGuy_Basketfall_01",             "round_basketfall_s4_show" },
            { "FallGuy_TerritoryControl_v2",       "round_territory_control_s4_show" },

            { "FallGuy_KingOfTheHill2",            "round_king_of_the_hill" },
            { "FallGuy_DrumTop",                   "round_drumtop" },
            { "FallGuy_Penguin_Solos",             "round_penguin_solos" },
            { "FallGuy_Gauntlet_07",               "round_gauntlet_07" },
            { "FallGuy_RobotRampage_Arena2",       "round_robotrampage_arena_2" },
            { "FallGuy_FruitBowl",                 "round_fruit_bowl" },
            { "FallGuy_Crown_Maze_Topdown",        "round_crown_maze" },

            { "FallGuy_Airtime",                   "round_airtime" },
            { "FallGuy_SeeSaw360",                 "round_see_saw_360" },
            { "FallGuy_FollowTheLeader",           "round_follow-the-leader_s6_launch" },
            { "FallGuy_Gauntlet_08",               "round_gauntlet_08" },
            { "FallGuy_PipedUp",                   "round_pipedup_s6_launch" },
            { "FallGuy_Invisibeans",               "round_invisibeans" },

            { "FallGuy_BlueJay",                   "round_bluejay" },
            { "FallGuy_HoopsRevenge",              "round_hoops_revenge_symphony_launch_show" },
            { "FallGuy_ShortCircuit2",             "round_short_circuit_2_symphony_launch_show" },
            { "FallGuy_Gauntlet_09",               "round_gauntlet_09_symphony_launch_show" },
            { "FallGuy_SpinRing",                  "round_spin_ring_symphony_launch_show" },
            { "FallGuy_1v1_Volleyfall",            "round_1v1_volleyfall_symphony_launch_show" },
            { "FallGuy_BlastBall_ArenaSurvival",   "round_blastball_arenasurvival_symphony_launch_show" },
            { "FallGuy_HexARing",                  "round_hexaring_symphony_launch_show" },

            { "FallGuy_SatelliteHoppers",          "round_satellitehoppers_almond" },
            { "FallGuy_FFA_Button_Bashers",        "round_ffa_button_bashers_squads_almond" },
            { "FallGuy_Hoverboard_Survival_2",     "round_hoverboardsurvival2_almond" },
            { "FallGuy_PixelPerfect",              "round_pixelperfect_almond" },
            { "FallGuy_Gauntlet_10",               "round_gauntlet_10_almond" },
            { "FallGuy_Starlink",                  "round_starlink_almond" },
            { "FallGuy_HexSnake",                  "round_hexsnake_almond" },
            { "FallGuy_Tip_Toe_Finale",            "round_tiptoefinale_almond" },

            { "FallGuy_BlastBallRuins",            "round_blastballruins" },
            { "FallGuy_FollowTheLine",             "round_follow_the_line" },
            { "FallGuy_Kraken_Attack",             "round_kraken_attack" },
            { "FallGuy_SlippySlide",               "round_slippy_slide" },
            { "FallGuy_SlideChute",                "round_slide_chute" },
        };
        public Image RoundIcon { get; set; }
        public Image RoundBigIcon { get; set; }
        public string Name { get; set; }
        public int Qualified { get; set; }
        public int Gold { get; set; }
        public int Silver { get; set; }
        public int Bronze { get; set; }
        public int Played { get; set; }
        public int Kudos { get; set; }
        public TimeSpan Fastest { get; set; }
        public TimeSpan Longest { get; set; }
        public int AveKudos { get { return this.Kudos / (this.Played == 0 ? 1 : this.Played); } }
        public TimeSpan AveDuration { get { return TimeSpan.FromSeconds((int)this.Duration.TotalSeconds / (this.Played == 0 ? 1 : this.Played)); } }
        public TimeSpan AveFinish { get { return TimeSpan.FromSeconds((double)this.FinishTime.TotalSeconds / (this.FinishedCount == 0 ? 1 : this.FinishedCount)); } }
        public LevelType Type;
        public BestRecordType BestRecordType;
        public bool IsCreative;
        public bool IsFinal;

        public TimeSpan Duration;
        public TimeSpan FinishTime;
        public List<RoundInfo> Stats;
        public int Season;
        public int FinishedCount;

        public LevelStats(string levelName, LevelType type, BestRecordType recordType, bool isCreative, bool isFinal, int season, Image roundIcon, Image roundBigIcon) {
            this.RoundIcon = roundIcon;
            this.RoundBigIcon = roundBigIcon;
            this.Name = levelName;
            this.Type = type;
            this.BestRecordType = recordType;
            this.Season = season;
            this.IsCreative = isCreative;
            this.IsFinal = isFinal;
            this.Stats = new List<RoundInfo>();
            this.Clear();
        }
        public void Clear() {
            this.Qualified = 0;
            this.Gold = 0;
            this.Silver = 0;
            this.Bronze = 0;
            this.Played = 0;
            this.Kudos = 0;
            this.FinishedCount = 0;
            this.Duration = TimeSpan.Zero;
            this.FinishTime = TimeSpan.Zero;
            this.Fastest = TimeSpan.Zero;
            this.Longest = TimeSpan.Zero;
            this.Stats.Clear();
        }
        public void Add(RoundInfo stat) {
            this.Stats.Add(stat);

            if (!stat.PrivateLobby || (stat.UseShareCode && !ALL.ContainsKey(stat.Name))) {
                this.Played++;
                this.Duration += stat.End - stat.Start;
                switch (stat.Tier) {
                    case (int)QualifyTier.Gold:
                        this.Gold++;
                        break;
                    case (int)QualifyTier.Silver:
                        this.Silver++;
                        break;
                    case (int)QualifyTier.Bronze:
                        this.Bronze++;
                        break;
                }

                this.Kudos += stat.Kudos;
                this.Qualified += stat.Qualified ? 1 : 0;
            }

            TimeSpan finishTime = stat.Finish.GetValueOrDefault(stat.Start) - stat.Start;
            if (finishTime.TotalSeconds > 1.1) {
                this.FinishedCount++;
                this.FinishTime += finishTime;
                if (this.Fastest == TimeSpan.Zero || this.Fastest > finishTime) {
                    this.Fastest = finishTime;
                }
                if (this.Longest < finishTime) {
                    this.Longest = finishTime;
                }
            }
        }

        public override string ToString() {
            return $"{this.Name}: {this.Qualified} / {this.Played}";
        }
    }
}