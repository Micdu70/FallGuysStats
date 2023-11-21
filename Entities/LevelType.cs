using System.Drawing;
using MetroFramework;

namespace FallGuysStats {
    public enum LevelType {
        CreativeRace,
        Race,
        Survival,
        Hunt,
        Logic,
        Team,
        Final,
        Invisibeans,
        Creative,
        Unknown
    }
    public enum BestRecordType {
        Fastest,
        Longest,
        HighScore
    }

    internal static class LevelTypeBehavior {
        public static int FastestLabel(this LevelType type) {
            switch (type) {
                case LevelType.CreativeRace:
                case LevelType.Race:
                case LevelType.Hunt:
                case LevelType.Invisibeans:
                case LevelType.Creative:
                    return 1; // FASTEST
                case LevelType.Survival:
                case LevelType.Logic:
                    return 0; // LONGEST
                case LevelType.Team:
                    return 2; // HIGH_SCORE
            }
            return 1;
        }
        public static string LevelTitle(this LevelType type, bool isFinal) {
            if (isFinal && type != LevelType.Invisibeans) {
                return Multilingual.GetWord("level_detail_final");
            }
            switch (type) {
                case LevelType.CreativeRace:
                case LevelType.Race:
                    return Multilingual.GetWord("level_detail_race");
                case LevelType.Survival:
                    return Multilingual.GetWord("level_detail_survival");
                case LevelType.Hunt:
                    return Multilingual.GetWord("level_detail_hunt");
                case LevelType.Logic:
                    return Multilingual.GetWord("level_detail_logic");
                case LevelType.Team:
                    return Multilingual.GetWord("level_detail_team");
                case LevelType.Invisibeans:
                    return Multilingual.GetWord("level_detail_invisibeans");
                case LevelType.Creative:
                    return Multilingual.GetWord("level_detail_creative");
            }
            return "Unknown";
        }
        public static Color LevelDefaultColor(this LevelType type, bool isFinal) {
            if (isFinal && type != LevelType.Invisibeans) {
                return Color.FromArgb(250, 195, 0);
            }
            switch (type) {
                case LevelType.CreativeRace:
                case LevelType.Race:
                    return Color.FromArgb(0, 235, 105);
                case LevelType.Survival:
                    return Color.FromArgb(185, 20, 210);
                case LevelType.Hunt:
                    return Color.FromArgb(45, 100, 190);
                case LevelType.Logic:
                    return Color.FromArgb(90, 180, 190);
                case LevelType.Team:
                    return Color.FromArgb(250, 80, 0);
                case LevelType.Invisibeans:
                    return Color.FromArgb(0, 0, 0);
                case LevelType.Creative:
                    return Color.FromArgb(255, 0, 165);
            }
            return Color.DarkGray;
        }
        public static Color LevelBackColor(this LevelType type, bool isFinal, bool isTeam, int alpha) {
            if (isFinal && type != LevelType.Invisibeans) {
                return Color.FromArgb(alpha, 250, 195, 0);
            }
            if (isTeam) {
                return Color.FromArgb(alpha, 250, 80, 0);
            }
            switch (type) {
                case LevelType.CreativeRace:
                case LevelType.Race:
                    return Color.FromArgb(alpha, 0, 235, 105);
                case LevelType.Survival:
                    return Color.FromArgb(alpha, 185, 20, 210);
                case LevelType.Hunt:
                    return Color.FromArgb(alpha, 45, 100, 190);
                case LevelType.Logic:
                    return Color.FromArgb(alpha, 90, 180, 190);
                case LevelType.Team:
                    return Color.FromArgb(alpha, 250, 80, 0);
                case LevelType.Invisibeans:
                    return Color.FromArgb(alpha, 0, 0, 0);
                case LevelType.Creative:
                    return Color.FromArgb(alpha, 255, 0, 165);
            }
            return Color.DarkGray;
        }
        public static Color LevelForeColor(this LevelType type, bool isFinal, bool isTeam, MetroThemeStyle theme = MetroThemeStyle.Default) {
            if (isFinal && type != LevelType.Invisibeans) {
                return Color.FromArgb(130, 100, 0);
            }
            if (isTeam) {
                return Color.FromArgb(130, 40, 0);
            }
            switch (type) {
                case LevelType.CreativeRace:
                case LevelType.Race:
                    return Color.FromArgb(0, 130, 55);
                case LevelType.Survival:
                    return Color.FromArgb(110, 10, 130);
                case LevelType.Hunt:
                    return Color.FromArgb(30, 70, 130);
                case LevelType.Logic:
                    return Color.FromArgb(60, 120, 130);
                case LevelType.Team:
                    return Color.FromArgb(130, 40, 0);
                case LevelType.Invisibeans:
                    return theme == MetroThemeStyle.Light ? Color.FromArgb(0, 0, 0) : Color.DarkGray;
                case LevelType.Creative:
                    return Color.FromArgb(130, 0, 85);
            }
            return Color.FromArgb(60, 60, 60);
        }
    }
}