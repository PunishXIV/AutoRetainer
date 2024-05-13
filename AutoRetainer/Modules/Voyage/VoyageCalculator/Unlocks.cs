namespace AutoRetainer.Modules.Voyage.VoyageCalculator;

public static class Unlocks
{
		public record UnlockedFrom(uint Point, bool Sub = false, bool Map = false);

		public static readonly Dictionary<uint, UnlockedFrom> PointToUnlockPoint = new()
{
		{ 0, new UnlockedFrom(9999) },              // Map A
    { 1, new UnlockedFrom(9000) },              // A    Default
    { 2, new UnlockedFrom(9000) },              // B    Default
    { 3, new UnlockedFrom(1) },                 // C    Deep-sea Site 2		        <-      The Ivory Shoals
    { 4, new UnlockedFrom(2) },                 // D    The Lightless Basin		    <-      Deep-sea Site 1
    { 5, new UnlockedFrom(2) },                 // E    Deep-sea Site 3		        <-      Deep-sea Site 1
    { 6, new UnlockedFrom(3) },                 // F    The Southern Rimilala Trench	<-      Deep-sea Site 2
    { 7, new UnlockedFrom(4) },                 // G    The Umbrella Narrow		    <-      The Lightless Basin
    { 8, new UnlockedFrom(7) },                 // H    Offender's Rot		            <-      The Umbrella Narrow
    { 9, new UnlockedFrom(5) },                 // I    Neolith Island		            <-      Deep-sea Site 3
    { 10, new UnlockedFrom(5, Sub: true) },     // J    Unidentified Derelict		    <-      Deep-sea Site 3
    { 11, new UnlockedFrom(9) },                // K    The Cobalt Shoals		        <-      Neolith Island
    { 12, new UnlockedFrom(8) },                // L    The Mystic Basin		        <-      Offender's Rot
    { 13, new UnlockedFrom(8) },                // M    Deep-sea Site 4		        <-      Offender's Rot
    { 14, new UnlockedFrom(10) },               // N    The Central Rimilala Trench	<-      Unidentified Derelict
    { 15, new UnlockedFrom(14, Sub: true) },    // O    The Wreckage Of Discovery I	<-      The Central Rimilala Trench
    { 16, new UnlockedFrom(11) },               // P    Komura		                    <-      The Cobalt Shoals
    { 17, new UnlockedFrom(16) },               // Q    Kanayama		                <-      Komura
    { 18, new UnlockedFrom(12) },               // R    Concealed Bay		            <-      The Mystic Basin
    { 19, new UnlockedFrom(15) },               // S    Deep-sea Site 5		        <-      The Wreckage Of Discovery I
    { 20, new UnlockedFrom(19, Sub: true) },    // T    Purgatory		                <-      Deep-sea Site 5
    { 21, new UnlockedFrom(19) },               // U    Deep-sea Site 6		        <-      Deep-sea Site 5
    { 22, new UnlockedFrom(21) },               // V    The Rimilala Shelf		        <-      Deep-sea Site 6
    { 23, new UnlockedFrom(14) },               // W    Deep-sea Site 7		        <-      The Central Rimilala Trench
    { 24, new UnlockedFrom(23) },               // X    Glittersand Basin		        <-      Deep-sea Site 7
    { 25, new UnlockedFrom(20) },               // Y    Flickering Dip		            <-      Purgatory
    { 26, new UnlockedFrom(25) },               // Z    The Wreckage Of The Headway	<-      Flickering Dip
    { 27, new UnlockedFrom(26) },               // AA   The Upwell		                <-      The Wreckage Of The Headway
    { 28, new UnlockedFrom(27) },               // AB   The Rimilala Trench Bottom		<-      The Upwell
    { 29, new UnlockedFrom(27) },               // AC   Stone Temple		            <-      The Upwell
    { 30, new UnlockedFrom(28, Map: true) },    // AD   Sunken Vault		            <-      The Rimilala Trench Bottom

    { 31, new UnlockedFrom(9999) },             // Map B
    { 32, new UnlockedFrom(30) },               // A South Isle Of Zozonan		        <- 		Sunken Vault
    { 33, new UnlockedFrom(32) },               // B Wreckage Of The Windwalker		<- 		South Isle Of Zozonan
    { 34, new UnlockedFrom(33) },               // C North Isle Of Zozonan		        <- 		Wreckage Of The Windwalker
    { 35, new UnlockedFrom(34) },               // D Sea Of Ash 1		                <- 		North Isle Of Zozonan
    { 36, new UnlockedFrom(35) },               // E The Southern Charnel Trench		<- 		Sea Of Ash 1
    { 37, new UnlockedFrom(34) },               // F Sea Of Ash 2		                <- 		North Isle Of Zozonan
    { 38, new UnlockedFrom(37) },               // G Sea Of Ash 3		                <- 		Sea Of Ash 2
    { 39, new UnlockedFrom(38) },               // H Ascetic's Demise		            <- 		Sea Of Ash 3
    { 40, new UnlockedFrom(38) },               // I The Central Charnel Trench		<- 		Sea Of Ash 3
    { 41, new UnlockedFrom(40) },               // J The Catacombs Of The Father		<- 		The Central Charnel Trench
    { 42, new UnlockedFrom(39) },               // K Sea Of Ash 4		                <- 		Ascetic's Demise
    { 43, new UnlockedFrom(42) },               // L The Midden Pit		            <- 		Sea Of Ash 4
    { 44, new UnlockedFrom(40) },               // M The Lone Glove		            <- 		The Central Charnel Trench
    { 45, new UnlockedFrom(41) },               // N Coldtoe Isle	                	<- 		The Catacombs Of The Father
    { 46, new UnlockedFrom(45) },               // O Smuggler's Knot		            <- 		Coldtoe Isle
    { 47, new UnlockedFrom(43) },               // P The Open Robe	                	<- 		The Midden Pit
    { 48, new UnlockedFrom(36) },               // Q Nald'thal's Pipe	            	<- 		The Southern Charnel Trench
    { 49, new UnlockedFrom(47, Map: true) },    // R The Slipped Anchor	        	<- 		The Open Robe
    { 50, new UnlockedFrom(45) },               // S Glutton's Belly	            	<- 		Coldtoe Isle
    { 51, new UnlockedFrom(42) },               // T The Blue Hole		                <- 		Sea Of Ash 4

    { 52, new UnlockedFrom(9999) },             // Map C
    { 53, new UnlockedFrom(49) },               // A The Isle Of Sacrament		        <- 		The Slipped Anchor
    { 54, new UnlockedFrom(53) },               // B The Kraken's Tomb		            <- 		The Isle Of Sacrament
    { 55, new UnlockedFrom(53) },               // C Sea Of Jade 1		                <- 		The Isle Of Sacrament
    { 56, new UnlockedFrom(55) },               // D Rogo-Tumu-Here's Haunt		    <- 		Sea Of Jade 1
    { 57, new UnlockedFrom(55) },               // E The Stone Barbs		            <- 		Sea Of Jade 1
    { 58, new UnlockedFrom(56) },               // F Rogo-Tumu-Here's Repose		    <- 		Rogo-Tumu-Here's Haunt
    { 59, new UnlockedFrom(57) },               // G Tangaroa's Prow		            <- 		The Stone Barbs
    { 60, new UnlockedFrom(57) },               // H Sea Of Jade 2		                <- 		The Stone Barbs
    { 61, new UnlockedFrom(59) },               // I The Blind Sound		            <- 		Tangaroa's Prow
    { 62, new UnlockedFrom(59) },               // J Sea Of Jade 3		                <- 		Tangaroa's Prow
    { 63, new UnlockedFrom(61) },               // K Moergynn's Forge		            <- 		The Blind Sound
    { 64, new UnlockedFrom(61) },               // L Tangaroa's Beacon		            <- 		The Blind Sound
    { 65, new UnlockedFrom(62) },               // M Sea Of Jade 4		                <- 		Sea Of Jade 3
    { 66, new UnlockedFrom(65) },               // N The Forest Of Kelp		        <- 		Sea Of Jade 4
    { 67, new UnlockedFrom(64) },               // O Sea Of Jade 5		                <- 		Tangaroa's Beacon
    { 68, new UnlockedFrom(66) },               // P Bladefall Chasm		            <- 		The Forest Of Kelp
    { 69, new UnlockedFrom(64) },               // Q Stormport		                    <- 		Tangaroa's Beacon
    { 70, new UnlockedFrom(65) },               // R Wyrm's Rest		                <- 		Sea Of Jade 4
    { 71, new UnlockedFrom(69) },               // S Sea Of Jade 6		                <- 		Stormport
    { 72, new UnlockedFrom(70, Map: true) },    // T The Devil's Crypt		            <- 		Wyrm's Rest

    { 73, new UnlockedFrom(9999) },             // Map D
    { 74, new UnlockedFrom(72) },               // A Mastbound's Bounty		        <- 		The Devil's Crypt
    { 75, new UnlockedFrom(74) },               // B Sirensong Sea 1		            <- 		Mastbound's Bounty
    { 76, new UnlockedFrom(74) },               // C Sirensong Sea 2		            <- 		Mastbound's Bounty
    { 77, new UnlockedFrom(76) },               // D Anthemoessa		                <- 		Sirensong Sea 2
    { 78, new UnlockedFrom(75) },               // E Magos Trench		                <- 		Sirensong Sea 1
    { 79, new UnlockedFrom(75) },               // F Thrall's Unrest		            <- 		Sirensong Sea 1
    { 80, new UnlockedFrom(76) },               // G Crow's Drop		                <- 		Sirensong Sea 2
    { 81, new UnlockedFrom(77) },               // H Sirensong Sea 3		            <- 		Anthemoessa
    { 82, new UnlockedFrom(81) },               // I The Anthemoessa Undertow		    <- 		Sirensong Sea 3
    { 83, new UnlockedFrom(79) },               // J Sirensong Sea 4		            <- 		Thrall's Unrest
    { 84, new UnlockedFrom(83) },               // K Seafoam Tide		                <- 		Sirensong Sea 4
    { 85, new UnlockedFrom(83) },               // L The Beak		                    <- 		Sirensong Sea 4
    { 86, new UnlockedFrom(81) },               // M Seafarer's End		            <- 		Sirensong Sea 3
    { 87, new UnlockedFrom(82) },               // N Drifter's Decay		            <- 		The Anthemoessa Undertow
    { 88, new UnlockedFrom(84) },               // O Lugat's Landing		            <- 		Seafoam Tide
    { 89, new UnlockedFrom(85) },               // P The Frozen Spring		            <- 		The Beak
    { 90, new UnlockedFrom(87) },               // Q Sirensong Sea 5		            <- 		Drifter's Decay
    { 91, new UnlockedFrom(88) },               // R Tidewind Isle		                <- 		Lugat's Landing
    { 92, new UnlockedFrom(88) },               // S Bloodbreak		                <- 		Lugat's Landing
    { 93, new UnlockedFrom(89, Map: true) },    // T The Crystal Font		            <- 		The Frozen Spring

    { 94, new UnlockedFrom(9999) },             // Map E
    { 95, new UnlockedFrom(93) },               // Weeping Trellis                     <-       The Crystal Font
    { 96, new UnlockedFrom(95) },               // The Forsaken Isle                   <-       Weeping Trellis
    { 97, new UnlockedFrom(95) },               // Fortune's Ford                      <-       Weeping Trellis
    { 98, new UnlockedFrom(96) },               // The Lilac Sea 1                     <-       The Forsaken Isle
    { 99, new UnlockedFrom(97) },               // Runner's Reach                      <-       Fortune's Ford
    { 100, new UnlockedFrom(96) },              // Bellflower Flood                    <-       The Forsaken Isle
    { 101, new UnlockedFrom(97) },              // The Lilac Sea 2                     <-       Fortune's Ford
    { 102, new UnlockedFrom(101) },                         // Lilac Sea 3                         <-       Lilac Sea 2
    { 103, new UnlockedFrom(98) },                          // Northwest Bellflower                <-       Lilac Sea 1
    { 104, new UnlockedFrom(100) },                         // Corolla Isle                        <-       Bellflower Flood
    { 105, new UnlockedFrom(101) },                         // Southeast Bellflower                <-       Lilac Sea 2
};

		public static List<(uint, UnlockedFrom)> FindUnlockPath(uint finalPoint)
		{
				if (!PointToUnlockPoint.TryGetValue(finalPoint, out var final))
						return [];

				// Unknown unlock at the time
				if (final.Point == 9876)
						return [];

				var wayPoints = new List<(uint, UnlockedFrom)> { (finalPoint, final) };

				var point = final.Point;
				while (point != 9000)
				{
						var newPoint = PointToUnlockPoint[point];
						wayPoints.Add((point, newPoint));

						point = newPoint.Point;
				}

				return wayPoints;
		}
}
