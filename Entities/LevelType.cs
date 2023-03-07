using System.Drawing;

namespace FallGuysStats {
    public enum LevelType {
        Race,
        Survival,
        SurvivalRace,
        Hunt,
        HuntScore,
        Logic,
        Team,
        TeamTime,
        Final,
        Invisibeans,
        Unknown
    }
    static class LevelTypeBehavior {
        public static int FastestLabel(this LevelType type) {
            switch (type) {
                case LevelType.Race:
                case LevelType.SurvivalRace:
                case LevelType.Hunt:
                case LevelType.TeamTime:
                case LevelType.Invisibeans:
                    return 1; // FASTEST
                case LevelType.Survival:
                case LevelType.Logic:
                    return 0; // LONGEST
                case LevelType.HuntScore:
                case LevelType.Team:
                    return 2; // HIGH_SCORE
            }
            return 1;
        }
        public static Color LevelBackColor(this LevelType type, bool isFinal, int alpha) {
            if (isFinal) {
                return Color.FromArgb(alpha, 249, 197, 3);
            }
            switch (type) {
                case LevelType.Race:
                    return Color.FromArgb(alpha, 5, 224, 109);
                case LevelType.Survival:
                case LevelType.SurvivalRace:
                    return Color.FromArgb(alpha, 182, 27, 210);
                case LevelType.Hunt:
                case LevelType.HuntScore:
                    return Color.FromArgb(alpha, 48, 101, 184);
                case LevelType.Logic:
                    return Color.FromArgb(alpha, 0, 153, 153);
                case LevelType.Team:
                case LevelType.TeamTime:
                    return Color.FromArgb(alpha, 245, 83, 3);
                case LevelType.Invisibeans:
                    return Color.FromArgb(alpha, 0, 0, 0);
            }
            return Color.DarkGray;
        }
        public static Color LevelForeColor(this LevelType type, bool isFinal) {
            if (isFinal) {
                return Color.FromArgb(149, 118, 1);
            }
            switch (type) {
                case LevelType.Race:
                    return Color.FromArgb(3, 134, 65);
                case LevelType.Survival:
                case LevelType.SurvivalRace:
                    return Color.FromArgb(109, 16, 126);
                case LevelType.Hunt:
                case LevelType.HuntScore:
                    return Color.FromArgb(28, 60, 110);
                case LevelType.Logic:
                    return Color.FromArgb(0, 91, 91);
                case LevelType.Team:
                case LevelType.TeamTime:
                    return Color.FromArgb(147, 49, 1);
                case LevelType.Invisibeans:
                    return Color.FromArgb(0, 0, 0);
            }
            return Color.DarkGray;
        }
    }
}