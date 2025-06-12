# Otherworld-Legends-Mod Advanced Tutorial
Please read [this tutorial](README.md) first.

Author: Kolyn090

Game version used in this tutorial: v2.9.1

Windows x64

Date: 5/15/2025

This tutorial intends to teach you how to modify game character's Spritesheet 
**so that you can edit beyond the boundary of the source Sprite.**

![advance_in_game_test](/images/advanced/advance_in_game_test.png)
(The final result you will get by the end of this tutorial)

⚡ **IMPORTANT**: Modding comes with unknown risks, including potential game 
instability, corrupted save files, compatibility issues, and even security 
vulnerabilities. Always back up your game files and proceed with caution.
Proceed at your own risk – I can't take responsibility if things go sideways.

⚡ **IMPORTANT**: This is a complex tutorial. Please read and follow each step 
carefully. Send me an issue ticket if you encountered problems that you don't
understand.

### Step 1
Firstly, download [Unity 2021.3.33f1](https://unity.com/releases/editor/whats-new/2021.3.33).
You won't need any Unity modules. I also strongly suggest you to learn the basics
of Unity first before proceed any further.

### Step 2
Create a new Unity Project. Choose Unity 3D as template. After you created the
project, create the following folders in Assets.

![unity_create_project](/images/advanced/unity_create_project.png)

Inside 'Dumps', create 'Game Dump' and 'My Dump'.

![dumps_folders](/images/advanced/dumps_folders.png)

For the purpose of teaching, this tutorial will show you how to replace the idle
state spritesheets of Quanhuying's bartender skin to the idle state spritesheet of
the Tianrendao boss. Please do bear in mind that you can definitely apply this technique 
with your own drawings. Using Tianrendao boss is only an example.

Their names in the spritereference folder are:

unit_asura_tianrendao.asset

unit_hero_quanhuying_bartender.asset

Use AssetStudio to extract their Texture2D files. Put them inside 'Texture2D' in Unity.

![unity_texture2d](/images/advanced/unity_texture2d.png)

Next select those two files and make the following adjustments in the Inspector.

![texture2d_parameter](/images/advanced/texture2d_parameter.png)

Make sure:
1. Texture Type = Sprite (2D and UI)
2. Sprite Mode = Multiple
3. Mesh TYpe = Full Rect
4. Uncheck Generate Physics Shape
5. Check Read/Write
6. Filter Mode = Point(no filter)
7. Compression = None
8. Apply

Next, use AssetStudio to extract all Dump files.

![extract_dumps](/images/advanced/extract_dumps.png)

Here are the extracted contents. Our focus is on the Sprite folder.

![extracted_folders](/images/advanced/extracted_folders.png)

Do that for both Quanhuying bartender and Tianrendao Sprite.
You should get two Sprite folders. Here I have renamed them to
'Quanhuyin_Sprite' (spelling mistake) and 'Tianrendao_Sprite' 
and put them inside the Game Dump folder.

![game_bundles](/images/advanced/game_bundles.png)

### Step 3
Next, download Unity's Sprite Editor. Open Package manager.

![package_manager_location](/images/advanced/package_manager_location.png)

Make sure you set Packages to Unity Registry.

![unity_reg](/images/advanced/unity_reg.png)

Find Features -> 2D -> 2D Sprite and download that.

![download_sprite2d](/images/advanced/download_sprite2d.png)

### Step 4

Next we will slice the Texture2D into multiple Sprites. 
Find the [BatchSpriteDumpImporter.cs](/scripts/BatchSpriteDumpImporter.cs)
script from the 'scripts' folder in this tutorial repo。

Put the csharp script (manually verified AI code✅) inside the 
Editor folder in Unity and wait for Unity to execute the code.

![code_sprite_importer](/images/advanced/code_sprite_importer.png)

You should see a new option in the Unity menu bar called Tools.
Choose Batch Import Sprites From Dumps.

![tools](/images/advanced/tools.png)

It will show you a popup window. Drag the Tianrendao boss Texture2D file's icon
into Target Texture, then drag Tianrendao_Sprite folder's icon to Dump Folder.
Click 'Import All Dumps'.

![use_sprite_importer](/images/advanced/use_sprite_importer.png)

You should now see a small triangular icon next to Texture2D. If you click that
you can see all sliced Sprites.

![sliced_tianrendao](/images/advanced/sliced_tianrendao.png)

(You can check in the Sprite Editor as well.)

![where_is_sprite_editor](/images/advanced/where_is_sprite_editor.png)

![sprite_editor](/images/advanced/sprite_editor.png)

Apply this technique on Quanhuying's bartender Texture2D as well. 

### Step 5
Next we will be editing the bartender skin (replacing the idle state to Tianrendao boss).
Put the two Texture2D in any Pixel art editing software. (Here I will demonstrate using
Aesprite).

It should look like this following (you will need to expand the canvas because Tianrendao
boss is taller than Quanhuying)

![draw](/images/advanced/draw.png)

Save this image.

Back to Unity, if you click the small triangular icon next to bartender's Texture2D. You
should find the first 7 sprites changed. However, we still need more adjustments.

![texture2d_edit_mismatch](/images/advanced/texture2d_edit_mismatch.png)

Now this next thing might be hard to understand. We will need to change the Sprtisheet
setting for the edited Sprites. Essentially copying the settings from the original
Tianrendao boss to the edited Sprites. ⚠️If you are doing your own drawings, you need to
manually adjust the Spritesheet and test them so that they look right. I won't be
demonstrating how to do that here. Try search how to use Sprite Editor. After you are done,
go to step 6.

Create a new folder inside 'Game Dump', call it 'Temp_Sprite'. From 'Tianrendao_Sprite',
copy the first seven dumps to there, and also copy from 'Quanhuyin_Sprite' (all of them except
the first seven). 

![temp_sprite](/images/advanced/temp_sprite.png)
(The red part is Tianrendao boss and the rest of them are Quanhuying)

Next, copy [RenameTextAssets.cs](/scripts/RenameTextAssets.cs) (manually verified AI code✅) 
from 'scripts'. After Unity executes it, you should find a new option called
Rename TextAssets Window in Tools.

![rename_text_assets](/images/advanced/rename_text_assets.png)

Next drag the first 7 dumps that belongs to Tianrendao boss to the popup window.
Enter name 'unit_hero_quanhuying_bartender'. Click 'Rename Files and Update Contents'.
By doing so we have renamed the files.

![rename](/images/advanced/rename.png)

Using Batch Import Sprites From Dumps, choose Folder 'Temp_Sprite' and bartender Texture2D.

![temp_sprite_usage](/images/advanced/temp_sprite_usage.png)

Open bartender's Sprite Editor.

![edit_uvmap](/images/advanced/edit_uvmap.png)

Align the Spritesheet for Tianrendao and apply.

![tianrendao_sprite_aligned](/images/advanced/tianrendao_sprite_aligned.png)

Check bartender again.

![check_sprite](/images/advanced/check_sprite.png)

### Step 6
In this step we will learn how to make bundles. Select bartender Texture2D and 
find this option.

![make_bundle](/images/advanced/make_bundle.png)

Choose 'New...', enter 'unit_hero_quanhuying_bartender'.

![new_bundle_name](/images/advanced/new_bundle_name.png)

Find [AssetBundleBuilder.cs](/scripts/AssetBundleBuilder.cs) in scripts folder and
put that in Unity Editor. After Unity executes it, you should see a new option
call Build Bundles. Now select bartender Texture2D and click that option.

![build_bundles](/images/advanced/build_bundles.png)

After it finishes, you will see a few new files inside 'AssetBundles'. 
There is a file called 'unit_hero_quanhuying_bartender' we can open
with UABEA.

![built_bundles](/images/advanced/built_bundles.png)

### Step 7
Open 'unit_hero_quanhuying_bartender' inside UABEA. Choose Memory -> Info.
Sort them by name.

![my_bundle_uabea](/images/advanced/my_bundle_uabea.png)

Select the following files.
1. unit_hero_quanhuying_bartender_0
2. unit_hero_quanhuying_bartender_1
3. unit_hero_quanhuying_bartender_2
4. unit_hero_quanhuying_bartender_3
5. unit_hero_quanhuying_bartender_4
6. unit_hero_quanhuying_bartender_5
7. unit_hero_quanhuying_bartender_6

After you did that, click 'Export Dump', put them inside Unity 'Dumps/My Dump'.
Save as UABE text temp.

![dump_from_my_bundle](/images/advanced/dump_from_my_bundle.png)

Check your 'My Dump'.

![check_exported_my_bundle](/images/advanced/check_exported_my_bundle.png)

Create a new folder called 'Source Dump' inside 'Dumps'.

![new_dump](/images/advanced/new_dump.png)

Open a new UABEA window, and from there open 'spritereference/unit_hero_quanhuyingbartender' file.
Choose an arbitrary unit_hero_quanhuying_bartender file and Export Dump into the Source Dump.

![uabea_for_source](/images/advanced/uabea_for_source.png)

Check your 'Source Dump'.

![check_source_dump](/images/advanced/check_source_dump.png)

Find [ReplacePathID38.cs](/scripts/ReplacePathID38.cs) (manually verified AI code✅) and put that
in Unity Editor folder. After Unity executes that you should see a new option called 
'Replace Line 38 From Folder'. Match 'Source Dump' and 'My Dump' and click 'Replace Line 38'.

![replace_line38](/images/advanced/replace_line38.png)

(FYI: Line 38 in a TextAsset is the PathID of texture. We do this to change our bundle's
PathID to the source bundle's PathID.)

⚠️ After the replacement is complete. Delete all files inside 'Source Dump' to avoid bugs
next time you make mod.

### Step 8
Open a new UABEA window, and from there open 'spritereference/unit_hero_quanhuyingbartender' file.
Replace the Texture2D file. Plugins -> Edit Texture -> Ok -> Load. Select the edited Texture2D
in Unity project. Save process is the exact same as before.

Next open 'spritereference/unit_hero_quanhuyingbartender' file again. This time we want to replace
Sprtes.

Select unit_hero_quanhuying_bartender_0. Click 'Import Dump'.
Find the corresponding file in 'My Dump'.

![import_dump](/images/advanced/import_dump.png)

![corresponding_dump](/images/advanced/corresponding_dump.png)

Do the same with 1, 2, 3, 4, 5, 6, then apply the exact same save process.

After that, run the addrtool. Test it in the game.

![advance_in_game_test](/images/advanced/advance_in_game_test.png)

---

If you found this tutorial helpful, please consider giving this repo a Star.