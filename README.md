# Steam Acheivment Viewer

Just another Steam Achievement Viewer that supports custom grouping of acheivemnts. Some uses are game collection that group multiplegames acheivments together or seperating DLC achievments from the base game

Groups for Kingdom Hearts 1.5 collection.
![image info](/images/Categories.png)

Achievemnts View
![image info](/images/Achievements.png)

## Adding a new Mapping for a Steam Game

1. Create a file name {appid}.json where {appid} is the app id that matches your Steam Game
2. Create a Field with the Name of the Section you wish to create for a set of Achievements and set the value of that field to the set of achievement IDs you wish to group.
3. Steam Achievement IDs can be queried using the Steam API. (https://api.steampowered.com/ISteamUserStats/GetSchemaForGame/v2/). I suggest using [this website](https://steamapi.xpaw.me/#) to run the API queries.

### Example for Kingdom Hearts 1.5/2.5 colletion (app id 2552430). 
4 Sections are created in this example (which matxhes the above picture). The ACH_001 ids are the IDs for the Achievement returned by the Steam API. 
```
{
	"Kingdom Hearts 1.5 Final Mix": ["ACH_001","ACH_002","ACH_003","ACH_004","ACH_005","ACH_006","ACH_007","ACH_008","ACH_009","ACH_010","ACH_011","ACH_012","ACH_013","ACH_014","ACH_015","ACH_016","ACH_017","ACH_018","ACH_019","ACH_020","ACH_021","ACH_022","ACH_023","ACH_024","ACH_025","ACH_026","ACH_027","ACH_028","ACH_029","ACH_030","ACH_031","ACH_032","ACH_033","ACH_034","ACH_035","ACH_036","ACH_037","ACH_038","ACH_039","ACH_040","ACH_041","ACH_042","ACH_043","ACH_044","ACH_045","ACH_046","ACH_047","ACH_048","ACH_049","ACH_050","ACH_051","ACH_052","ACH_053","ACH_054","ACH_055"],
	"Kingdom Hearts Re: Chain of Memories": ["ACH_056","ACH_057","ACH_058","ACH_059","ACH_060","ACH_061","ACH_062","ACH_063","ACH_064","ACH_065","ACH_066","ACH_067","ACH_068","ACH_069","ACH_070","ACH_071","ACH_072","ACH_073","ACH_074","ACH_075","ACH_076","ACH_077","ACH_078","ACH_079","ACH_080","ACH_081","ACH_082","ACH_083","ACH_084","ACH_085","ACH_086","ACH_087","ACH_088","ACH_089","ACH_090","ACH_091","ACH_092","ACH_093","ACH_094","ACH_095","ACH_096","ACH_097","ACH_098","ACH_099","ACH_100","ACH_101","ACH_102"],
	"Kingdom Hearts 2.5 Final Mix": ["ACH_103","ACH_104","ACH_105","ACH_106","ACH_107","ACH_108","ACH_109","ACH_110","ACH_111","ACH_112","ACH_113","ACH_114","ACH_115","ACH_116","ACH_117","ACH_118","ACH_119","ACH_120","ACH_121","ACH_122","ACH_123","ACH_124","ACH_125","ACH_126","ACH_127","ACH_128","ACH_129","ACH_130","ACH_131","ACH_132","ACH_133","ACH_134","ACH_135","ACH_136","ACH_137","ACH_138","ACH_139","ACH_140","ACH_141","ACH_142","ACH_143","ACH_144","ACH_145","ACH_146","ACH_147","ACH_148","ACH_149","ACH_150","ACH_151","ACH_152"],
	"Kingdom Hearts Birth By Sleep": ["ACH_153","ACH_154","ACH_155","ACH_156","ACH_157","ACH_158","ACH_159","ACH_160","ACH_161","ACH_162","ACH_163","ACH_164","ACH_165","ACH_166","ACH_167","ACH_168","ACH_169","ACH_170","ACH_171","ACH_172","ACH_173","ACH_174","ACH_175","ACH_176","ACH_177","ACH_178","ACH_179","ACH_180","ACH_181","ACH_182","ACH_183","ACH_184","ACH_185","ACH_186","ACH_187","ACH_188","ACH_190","ACH_191","ACH_192","ACH_193","ACH_194","ACH_195","ACH_196","ACH_197","ACH_189"],
}

```
