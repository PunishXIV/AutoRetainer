using Lumina.Excel.Sheets;

namespace AutoRetainer.Modules.Voyage.VoyageCalculator;

// All this data is taken from:
// https://docs.google.com/spreadsheets/d/1-j0a-I7bQdjnXkplP9T4lOLPH2h3U_-gObxAiI4JtpA
// Credits to Mystic Spirit and other contributors from the submarine discord
public static class Sectors
{
    public record Breakpoint(int T2, int T3, int Normal, int Optimal, int Favor)
    {
        public static Breakpoint Empty => new(0, 0, 0, 0, 0);
    };

    public static readonly Dictionary<uint, Breakpoint> MapBreakpoints = new()
{
    { 001, new Breakpoint(020, 080, 050, 080, 070) },
    { 002, new Breakpoint(020, 080, 050, 080, 070) },
    { 003, new Breakpoint(020, 085, 055, 085, 070) },
    { 004, new Breakpoint(020, 085, 055, 085, 070) },
    { 005, new Breakpoint(025, 090, 060, 090, 080) },
    { 006, new Breakpoint(025, 090, 060, 090, 080) },
    { 007, new Breakpoint(030, 095, 065, 095, 090) },
    { 008, new Breakpoint(030, 100, 070, 100, 090) },
    { 009, new Breakpoint(035, 110, 075, 105, 090) },
    { 010, new Breakpoint(050, 115, 080, 110, 090) },
    { 011, new Breakpoint(050, 090, 080, 110, 070) },
    { 012, new Breakpoint(055, 095, 090, 120, 080) },
    { 013, new Breakpoint(060, 100, 100, 130, 075) },
    { 014, new Breakpoint(060, 100, 100, 130, 085) },
    { 015, new Breakpoint(080, 115, 120, 160, 090) },
    { 016, new Breakpoint(060, 100, 100, 130, 085) },
    { 017, new Breakpoint(065, 105, 110, 140, 090) },
    { 018, new Breakpoint(085, 120, 135, 175, 095) },
    { 019, new Breakpoint(075, 110, 120, 155, 095) },
    { 020, new Breakpoint(090, 125, 140, 180, 100) },
    { 021, new Breakpoint(090, 120, 135, 175, 095) },
    { 022, new Breakpoint(105, 130, 140, 180, 100) },
    { 023, new Breakpoint(110, 140, 140, 180, 105) },
    { 024, new Breakpoint(120, 130, 145, 190, 105) },
    { 025, new Breakpoint(120, 135, 145, 190, 105) },
    { 026, new Breakpoint(135, 140, 150, 195, 110) },
    { 027, new Breakpoint(130, 145, 150, 195, 110) },
    { 028, new Breakpoint(130, 150, 155, 200, 120) },
    { 029, new Breakpoint(135, 150, 160, 200, 130) },
    { 030, new Breakpoint(140, 155, 170, 215, 135) },

    { 032, new Breakpoint(135, 150, 165, 205, 140) },
    { 033, new Breakpoint(140, 155, 170, 205, 140) },
    { 034, new Breakpoint(140, 160, 175, 210, 145) },
    { 035, new Breakpoint(145, 165, 180, 220, 145) },
    { 036, new Breakpoint(145, 160, 185, 220, 150) },
    { 037, new Breakpoint(145, 165, 180, 220, 145) },
    { 038, new Breakpoint(150, 170, 180, 220, 140) },
    { 039, new Breakpoint(160, 175, 190, 225, 150) },
    { 040, new Breakpoint(155, 170, 190, 220, 140) },
    { 041, new Breakpoint(160, 175, 190, 225, 150) },
    { 042, new Breakpoint(155, 170, 185, 230, 160) },
    { 043, new Breakpoint(160, 175, 185, 235, 165) },
    { 044, new Breakpoint(160, 170, 190, 240, 175) },
    { 045, new Breakpoint(165, 190, 195, 245, 170) },
    { 046, new Breakpoint(170, 185, 205, 250, 175) },
    { 047, new Breakpoint(165, 180, 185, 235, 165) },
    { 048, new Breakpoint(165, 180, 185, 235, 165) },
    { 049, new Breakpoint(170, 185, 190, 240, 165) },
    { 050, new Breakpoint(175, 190, 200, 250, 175) },
    { 051, new Breakpoint(180, 190, 200, 250, 175) },

    { 053, new Breakpoint(180, 190, 200, 250, 175) },
    { 054, new Breakpoint(180, 190, 200, 250, 175) },
    { 055, new Breakpoint(180, 190, 200, 250, 175) },
    { 056, new Breakpoint(180, 195, 205, 260, 178) },
    { 057, new Breakpoint(180, 195, 210, 260, 185) },
    { 058, new Breakpoint(180, 195, 210, 265, 185) },
    { 059, new Breakpoint(180, 195, 215, 270, 185) },
    { 060, new Breakpoint(180, 195, 220, 270, 185) },
    { 061, new Breakpoint(180, 195, 220, 270, 185) },
    { 062, new Breakpoint(180, 195, 220, 270, 185) },
    { 063, new Breakpoint(185, 200, 225, 275, 190) },
    { 064, new Breakpoint(185, 200, 230, 280, 190) },
    { 065, new Breakpoint(185, 200, 230, 280, 190) },
    { 066, new Breakpoint(190, 205, 235, 285, 195) },
    { 067, new Breakpoint(195, 210, 240, 290, 200) },
    { 068, new Breakpoint(195, 210, 245, 295, 200) },
    { 069, new Breakpoint(200, 215, 255, 300, 205) },
    { 070, new Breakpoint(205, 220, 255, 300, 210) },
    { 071, new Breakpoint(205, 220, 260, 305, 210) },
    { 072, new Breakpoint(205, 220, 260, 305, 210) },

    { 074, new Breakpoint(205, 220, 260, 305, 210) },
    { 075, new Breakpoint(205, 220, 260, 305, 210) },
    { 076, new Breakpoint(205, 220, 260, 305, 210) },
    { 077, new Breakpoint(210, 225, 265, 310, 215) },
    { 078, new Breakpoint(210, 225, 265, 310, 215) },
    { 079, new Breakpoint(210, 225, 265, 310, 215) },
    { 080, new Breakpoint(210, 225, 265, 310, 215) },
    { 081, new Breakpoint(215, 230, 270, 315, 220) },
    { 082, new Breakpoint(215, 230, 270, 315, 220) },
    { 083, new Breakpoint(215, 230, 270, 315, 220) },
    { 084, new Breakpoint(215, 230, 270, 315, 220) },
    { 085, new Breakpoint(215, 230, 270, 315, 220) },
    { 086, new Breakpoint(215, 230, 270, 315, 220) },
    { 087, new Breakpoint(220, 235, 275, 320, 225) },
    { 088, new Breakpoint(220, 235, 275, 320, 225) },
    { 089, new Breakpoint(220, 235, 275, 320, 225) },
    { 090, new Breakpoint(220, 235, 275, 320, 225) },
    { 091, new Breakpoint(220, 235, 275, 320, 225) },
    { 092, new Breakpoint(220, 235, 275, 320, 225) },
    { 093, new Breakpoint(220, 235, 275, 320, 225) },

    { 095, new Breakpoint(220, 235, 275, 320, 225) },
    { 096, new Breakpoint(220, 235, 275, 320, 225) },
    { 097, new Breakpoint(220, 235, 275, 320, 225) },
    { 098, new Breakpoint(225, 240, 280, 325, 230) },
    { 099, new Breakpoint(225, 237, 280, 325, 227) },
    { 100, new Breakpoint(225, 238, 280, 325, 230) },
    { 101, new Breakpoint(225, 240, 280, 325, 230) },

    { 102, new Breakpoint(226,241,281,326,231) },
    { 103, new Breakpoint(227,242,282,327,232) },
    { 104, new Breakpoint(228,243,283,328,233) },
    { 105, new Breakpoint(229,244,284,329,234) },
    { 106, new Breakpoint(230,245,285,330,235) },
    { 107, new Breakpoint(230,245,285,330,235) },

    { 108, new Breakpoint(231,246,286,331,236) },
    { 109, new Breakpoint(232,247,287,332,237) },
    { 110, new Breakpoint(233,248,288,333,238) },
    { 111, new Breakpoint(234,249,289,334,239) },
    { 112, new Breakpoint(234,249,289,334,239) },
    { 113, new Breakpoint(235,250,290,335,240) },
    { 114, new Breakpoint(235,250,290,335,240) },
};

    public static Breakpoint CalculateBreakpoint(List<uint> points)
    {
        // more than 5 points isn't allowed ingame
        if(points.Count is 0 or > 5)
            return Breakpoint.Empty;

        var breakpoints = new List<Breakpoint>();
        foreach(var point in points)
        {
            if(!MapBreakpoints.TryGetValue(point, out var br))
                return Breakpoint.Empty;
            breakpoints.Add(br);
        }

        // every map can have different max, so we have to check every single one
        var t2 = breakpoints.Max(b => b.T2);
        var t3 = breakpoints.Max(b => b.T3);
        var normal = breakpoints.Max(b => b.Normal);
        var optimal = breakpoints.Max(b => b.Optimal);
        var favor = breakpoints.Max(b => b.Favor);

        return new Breakpoint(t2, t3, normal, optimal, favor);
    }

    public static List<(int Guaranteed, int Max)> PredictBonusExp(List<uint> sectors, Build.SubmarineBuild build)
    {
        var predictedExp = new List<(int, int)>();
        foreach(var sector in sectors)
            predictedExp.Add(PredictBonusExp(sector, build));

        return predictedExp;
    }

    public static (int Guaranteed, int Maximum) PredictBonusExp(uint sector, Build.SubmarineBuild build)
    {
        if(!MapBreakpoints.TryGetValue(sector, out var br))
            return (0, 0);

        var guaranteed = 0;
        guaranteed += br.Optimal <= build.Retrieval ? 1 : 0;

        var maximum = guaranteed;
        maximum += br.T2 <= build.Surveillance ? 1 : 0;
        maximum += br.T3 <= build.Surveillance ? 1 : 0;

        if(br.Favor <= build.Favor)
        {
            maximum += 1;
            maximum += br.T2 <= build.Surveillance ? 1 : 0;
            maximum += br.T3 <= build.Surveillance ? 1 : 0;
        }

        return (guaranteed, Math.Clamp(maximum, 0, 4));
    }

    public static uint CalculateExpForSectors(List<SubmarineExploration> sectors, Build.SubmarineBuild build)
    {
        var bonusEachSector = PredictBonusExp(sectors.Select(s => s.RowId).ToList(), build);
        if(!bonusEachSector.Any())
            return 0u;

        var expGain = 0u;
        foreach(var (bonus, sector) in bonusEachSector.Zip(sectors))
            expGain += CalculateBonusExp(bonus.Guaranteed, sector.ExpReward);

        return expGain;
    }

    public static uint CalculateBonusExp(int bonus, uint exp)
    {
        return (bonus) switch
        {
            0 => exp,
            1 => (uint)(exp * 1.25),
            2 => (uint)(exp * 1.50),
            3 => (uint)(exp * 1.75),
            4 => (uint)(exp * 2.00),
            _ => exp
        };
    }
}
