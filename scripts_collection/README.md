This is a collection of useful scripts that can speed up the work.

### [AssetBundleBuilder.cs](/scripts_collection/AssetBundleBuilder.cs)
Pack **all** AssetBundles in the project. The best practice is to tag only 
AssetBundles that are necessary. In this way you can save some time and 
resources. The output will be stored in 'Assets/AssetBundles'.

<u>When to use: after you are done with building the mod, the last step is
to pack the AssetBundles.</u>

为项目中所有AssetBundle打包。请只标注（tag）你目前需要的AssetBundle，这样可以 
节省时间和资源。生成文件会被放在‘Assets/AssetBundles’。

<u>何时使用：当你完成模组后， 最后一步就是打包。</u>

### [AssignSlicedSpritesToAtlas.cs](/scripts_collection/AssignSlicedSpritesToAtlas.cs)
Removes all currently packed packables in the atlas (by default) and load all sliced 
sprites in spritesheet to it. Use this tool to greatly speed up loading sprites into atlas 
because Unity 2021.3 doesn't seem to offer batch import but this script can do it.

<u>When to use: use this tool to put all Sprites from Texture2D to SpriteAtlas.</u>

移除所选择的atlas中所有的资源（初始设置），并从spritesheet载入所有切割过的sprite。该工具属于提升工作
效率类。Unity 2021.3 似乎并不提供多文件载入到atlas但是你可以使用该工具来快速完成。

<u>何时使用：当你想把Texture2D中的所有Sprites放到一个SpriteAtlas中，可以通过该工具快速完成。</u>

### [AutoSpriteSheetSliceFromFolder.cs](/scripts_collection/AutoSpriteSheetSliceFromFolder.cs)
Takes in a Texture2D and a folder of sliced sprites of it (The items in the
folder must be the result of slicing of this Texture2D), apply slicing on the
Texture2D based on the provided Sprites (It's like reversing the process from
sliced sprites to sliced Texture2D). Name and size of each sprite will be copied.

<u>When to use: use this tool to slice a Texture2D like in the original game.</u>

逆向sprite切割，用已经切割好的sprite来切割Texture2D。提供的sprite必须是要从
Texture2D中切割出的。所有sprite的名字，尺寸都会被复制。

<u>何时使用：当你想重现一个Texture2D在源游戏的切割，可以使用该工具。</u>

### [BatchSpriteDumpImporter.cs](/scripts_collection/BatchSpriteDumpImporter.cs)
Slice spritesheet through dumps. It's essentially functioning the same as 
AutoSpriteSheetSlicerFromFolder but requires less space to use and much
faster than AutoSpriteSheetSlicerFromFolder. Although I do recommend
AutoSpriteSheetSlicerFromFolder since that handles sprite offset. 

<u>When to use: use this tool to slice a Texture2D like in the original game.</u>

和AutoSpriteSheetSlicerFromFolder有着相同的功能不过该工具依赖dump文件。
运行速度比AutoSpriteSheetSlicerFromFolder要快的多。
dump文件比sprite更小所以该工具使用所需要的空间也更小。不过我更推荐使用
AutoSpriteSheetSlicerFromFolder因为那个工具可以自动载入每个sprite的位置，
而该工具则需要手动调位置。

<u>何时使用：当你想重现一个Texture2D在源游戏的切割，可以使用该工具。</u>

### [ModApplier.cs](/scripts_collection/ModApplier.cs)
Originally named 'ModApplier.cs'. Load the mod into the game. The mod must be
constructed in a specific way - each modified .bun file must be put in the 
location referring to its location in the game folder. Run this script to do
this for you. After the replacement, remember to do AddrTool. You will also 
need to keep your mod up to date (File location change & name change...)

<u>When to use: when your mod runs without problem in the actual game. You
can build a mod folder for faster mod application.</u>

原名‘ModApplier.cs’，载入模组到游戏。所有模组中的bun文件需要存放在一个模拟游戏
文件夹位置的文件夹中。运行该工具以替换游戏资源。然后记得运行AddrTool。你还需要
保证模组更新。

<u>何时使用：当你的模组已经可以在游戏中运行，可以建立一个mod文件夹然后通过该工具来实现快速
替换模组。</u>

### [RenameTextAssets.cs](/scripts_collection/RenameTextAssets.cs)
Rename all loaded TextAsset (sprite dumps) files. After renaming, it will use
number of suffix. Notice that the base name in TextAsset will also be changed.

<u>When to use: normally this tool is not used. One case I can think of is to
replace a game character sprite with another character in the game. This will
require BatchSpriteDumpImporter.cs. Please refer to the 
[Advanced tutorial](/README_advanced.md) for example.</u>

为所有载入的TextAsset文件（sprite dump）重新命名。修改名字后的文件会用数字作为后缀。
文件里的base name也会被更改。

<u>何时使用：通常情况下该工具不被使用。一种使用情况为替换一个游戏角色与另一个同游戏角色的sprite。
这需要和BatchSpriteDumpImporter.cs 一起使用。
具体示例可以参考[进阶教程](/README_进阶教程.md)</u>。

### [ReplacePathID38.cs](/scripts_collection/ReplacePathID38.cs)
Replace path id (of sprites TextAsset). This is located in line 38. We
want to replace this line with the path id in the original game resource. 
Also, this tool only work for spritesheets that do
not involve spriteatlas. If that were that case, you must use 
ReplacePathIDAtlas36.cs instead.

<u>When to use: after you have your own dump file. You still need to edit
the texture Path_ID. Find the source Path_ID (should locate in line 38 as well)
and run this tool. Note that you only need to find one source Path_ID 
because they are all the same in a bundle.</u>

替换sprite TextAsset的Path ID。该数据存储在文件中的第38行。该文件主要使用目的
为替换Path ID成源游戏资源的Path ID。注意：该工具不适用于有使用SpriteAtlas的文件，
你需要用对atlas中的文件用‘ReplacePathIDAtlas36.cs’。

<u>何时使用：当你生成自己的dump文件后，需要修改texture的Path_ID。找到源文件的Path_ID
（也是在第38行）然后运行该工具替换。你仅需要找到一个源文件Path_ID因为在同一个包里它们都是一样的。</u>

### [ReplacePathIDAltas36.cs](/scripts_collection/ReplacePathIDAltas36.cs)
Replace path id (of sprites TextAssest) in atlas. This is located in line 36. 
We want to replace this line with the path id in the original game resource. 
Warning: for non-atlas sprites, use ReplacePathID38.cs instead.

<u>When to use: after you have your own dump file. You still need to edit
the texture Path_ID. Find the source Path_ID (should locate in line 36 as well)
and run this tool. Note that you only need to find one source Path_ID 
because they are all the same in a bundle.</u>

替换atlas中的sprite TextAsset的Path ID。该数据存储在文件中的第38行。该文件主要使用目的
为替换Path ID成源游戏资源的Path ID。注意：该工具只适用于有使用SpriteAtlas的文件，
如果无使用atlas，则使用‘ReplacePathID38.cs’工具。

<u>何时使用：当你生成自己的dump文件后，需要修改texture的Path_ID。找到源文件的Path_ID
（也是在第36行）然后运行该工具替换。你仅需要找到一个源文件Path_ID因为在同一个包里它们都是一样的。
这个工具是[第二个进阶教程](/README_进阶教程2.md)中第三步骤的自动化版本。</u>


### [ReplacePathIDAtlas.cs](/scripts_collection/ReplacePathIDAtlas.cs)
⚠️ This is not the same as ReplacePathIDAtlas36.cs. This tool replaces the path ids
in the SpriteAtlas file's dump (a single txt file). Requires the source sprite atlas
dump file (for correct path ids in game).

<u>When to use: use this tool to automatically edit SpriteAtlas TextAsset.</u>

⚠️ 此工具和‘ReplacePathIDAtlas36.cs’使用目的不同。该工具替换SpriteAtlas文件（是一个单一的
txt文件）中的（多个）Path ID。另外，该工具需要源SpriteAtlas的dump文件。

<u>何时使用：当你需要修改SpriteAtlas的TextAsset文件（假设你的sprite是在atlas中）。
这个工具是[第二个进阶教程](/README_进阶教程2.md)中第三步骤的自动化版本。</u>
