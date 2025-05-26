# Otherworld-Legends-Mod 进阶教程
在阅读本教程前，请先学习[基础教程](README_中文.md)。

作者：Kolyn090

教程游戏版本：v2.9.1

使用Windows x64

教程日期：5/25/2025

本教程将指引你如何修改游戏角色图像（Sprite）的UV贴图。
**这样做的目的是为了让你可以不受原贴图的大小限制修改角色皮肤。**

⚡ **警告**：模组修改可能带来未知风险，包括游戏不稳定、存档损坏、兼容性问题，甚至安全漏洞。切记备份游戏文件，并谨慎操作。
风险自担——如果出现问题，我无法承担责任。

⚡ **警告**：该教程的操作流程非常复杂而且繁琐，请仔细阅读每一步。如果你遇到问题可以发issue提问。

### 步骤一
首先，你需要[Unity 2021.3.33f1](https://unity.com/releases/editor/whats-new/2021.3.33)。
下载并安装Unity Hub，选择Unity版本2021.3.33f1。可以不装任何模组（modules）。
如果你没有使用Unity的经验，建议先观看视频学习。

### 步骤二
创建一个新的Unity项目。模板选择Unity 3D，为项目命名然后创建。
在进入Unity后，创建以下文件夹。

![unity_create_project](/images/unity_create_project.png)


在‘Dumps’这个文件夹里面，创建两个新的文件夹。将其命名为‘Game Bundle’和
‘My Bundle’。

![dumps_folders](/images/dumps_folders.png)

出于教学目的，本教程仅展示将权虎鹰的马甲裙皮肤的初始状态修改为天人道boss的初始状态。

它们在spritereference的命名前缀为：

unit_asura_tianrendao.asset

unit_hero_quanhuying_bartender.asset

使用AssetStudio提取它们的Texture2D。并放入‘Texture2D’这个文件夹。

![unity_texture2d](/images/unity_texture2d.png)

接下来选择这两个文件，在Inspector里面修改它们的参数。

![texture2d_parameter](/images/texture2d_parameter.png)

请确保：
1. Texture Type是Sprite(2D and UI)
2. Sprite Mode是Multiple
3. Mesh Type是Full Rect
4. 关闭Generate Physics Shape
5. 开启Read/Write
6. Filter Mode是Point(no filter)
7. Compression是None
8. 点击Apply

接下来就会比较困难了。我们先使用AssetStudio提取所有Sprite的Dump文件。

![extract_dumps](/images/extract_dumps.png)

这是提取后的文件。我们只关心Sprite那个文件夹。
![extracted_folders](/images/extracted_folders.png)

把两个Sprite文件夹重新取名为‘Quanhuyin_Sprite’和‘Tianrendao_Sprite’。
再把它们放到Game Bundle里面。

![game_bundles](/images/game_bundles.png)

### 步骤三
现在我们需要下载Unity的Sprite Editor。打开Package manager（见图）。

![package_manager_location](/images/package_manager_location.png)

把Packages换成Unity Registry。

![unity_reg](/images/unity_reg.png)

找到 Features -> 2D -> 2D Sprite 并下载。

![download_sprite2d](/images/download_sprite2d.png)

### 步骤四

接下来我们需要将切割Texture2D成多个Sprite。在本教程的scripts文件夹里面，找到
[BatchSpriteDumpImporter.cs](/scripts/BatchSpriteDumpImporter.cs)。

把这个csharp代码（AI生成，已检查✅）放进Editor文件夹。放入后Unity会自动跑一下。

![code_sprite_importer](/images/code_sprite_importer.png)

然后你的Unity上方的工具栏会多一个Tools的选项。选择Batch Import Sprites From Dumps。

![tools](/images/tools.png)

你会看见一个弹窗，把天人道boss的Texture2D图标拖到Target Texture里面，
然后把Tianrendao_Sprite这个文件夹图标拖到Dump Folder。点击'Import All Dumps'。

![use_sprite_importer](/images/use_sprite_importer.png)

你应该会看见Texture2D旁边有一个三角形的小图标，点开来就能看见所有Sprite。

![sliced_tianrendao](/images/sliced_tianrendao.png)

（更直观一些，可以在Inspector里面选Sprite Editor查看。）

![where_is_sprite_editor](/images/where_is_sprite_editor.png)

![sprite_editor](/images/sprite_editor.png)

按照这个方法，把权虎鹰的马甲裙那份也切割一下。这里就不演示了。

### 步骤五
在这一步我们将编辑马甲裙皮肤（把初始状态换成天人道boss)。
把这两个Texture2D放进任意像素编辑软件里（不是专门像素的也行，我这里使用的是Aesprite）。

介于版权问题我就不放原画了，总之画成这个样子就行了。（因为天人道boss比较高，需要改一下图片大小）。
![draw](/images/draw.png)

改完图片之后可以直接保存。

回到Unity，点击马甲裙的小三角图标，会发现前七个Sprites有变化。不过我们还需要调整。

![texture2d_edit_mismatch](/images/texture2d_edit_mismatch.png)


这一步可能会有些难理解。我们需要修改新图像的UV贴图变换。这里要做的就是从天人道boss
的Sprite那边复制贴图变换到新的图像。

首先在‘Game Bundle’创建一个新的文件夹，叫’Temp_Sprite‘。从’Tianrendao_Sprite‘那里复制前七个
dumps到这里，然后从’Quanhuyin_Sprite‘那里复制除了前七个所有的dumps。简单来说就是换对应的dumps。

![temp_sprite](/images/temp_sprite.png)
（红色部分是天人道boss）


然后，在本教程的scripts里面找到RenameTextAssets.cs（AI生成，已检查✅），把它放进'Editor'。Unity运行完后
你会在Tools里找到一个叫Rename TextAssets Window的新选项。

![rename_text_assets](/images/rename_text_assets.png)

接下来把属于天人道boss的前七个dumps拖到这个弹窗里面，然后名字输入‘unit_hero_quanhuying_bartender’。
点击‘Rename Files and Update Contents’，这样文件的名字就全改好了。

![rename](/images/rename.png)

然后用Batch Import Sprites From Dumps，文件夹选择‘Temp_Sprite’，
Texture2D选择马甲裙。

![temp_sprite_usage](/images/temp_sprite_usage.png)

打开马甲裙的Sprite Editor。

![edit_uvmap](/images/edit_uvmap.png)

将天人道boss的Sprite对齐。点击Apply。

![tianrendao_sprite_aligned](/images/tianrendao_sprite_aligned.png)

