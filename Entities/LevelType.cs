using System.Drawing;

namespace FallGuysStats {
    public enum LevelType {
        Race,
        Survival,
        Hunt,
        Logic,
        Team,
        Final,
        Invisibeans,
        Unknown
    }
    static class LevelTypeBehavior {
        public static int FastestLabel(this LevelType type) {
            switch (type) {
                case LevelType.Race:
                case LevelType.Hunt:
                case LevelType.Invisibeans:
                    return 1; // FASTEST
                case LevelType.Survival:
                case LevelType.Logic:
                    return 0; // LONGEST
                case LevelType.Team:
                    return 2; // HIGH_SCORE
            }
            return 1;
        }
        public static Color LevelBackColor(this LevelType type, bool isFinal, int alpha) {
            if (isFinal) {
                return Color.FromArgb(alpha, 250, 200, 0);
            }
            switch (type) {
                case LevelType.Race:
                    return Color.FromArgb(alpha, 0, 235, 105);
                case LevelType.Survival:
                    return Color.FromArgb(alpha, 185, 20, 210);
                case LevelType.Hunt:
                    return Color.FromArgb(alpha, 45, 100, 185);
                case LevelType.Logic:
                    return Color.FromArgb(alpha, 90, 180, 190);
                case LevelType.Team:
                    return Color.FromArgb(alpha, 250, 80, 0);
                case LevelType.Invisibeans:
                    return Color.FromArgb(alpha, 0, 0, 0);
            }
            return Color.DarkGray;
        }
        public static Color LevelForeColor(this LevelType type, bool isFinal) {
            if (isFinal) {
                return Color.FromArgb(250, 210, 40);
            }
            switch (type) {
                case LevelType.Race:
                    return Color.FromArgb(40, 235, 125);
                case LevelType.Survival:
                    return Color.FromArgb(190, 50, 210);
                case LevelType.Hunt:
                    return Color.FromArgb(75, 120, 185);
                case LevelType.Logic:
                    return Color.FromArgb(120, 185, 190);
                case LevelType.Team:
                    return Color.FromArgb(250, 110, 40);
                case LevelType.Invisibeans:
                    return Color.FromArgb(0, 0, 0);
            }
            return Color.DarkGray;
        }
    }
}