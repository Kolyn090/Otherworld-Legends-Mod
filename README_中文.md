# Otherworld-Legends-Mod
本教程的初始版本由bilibili平台[咖喱猫扒饭撰写](https://b23.tv/IkX17vZ)。

🌟 跳转到[进阶教程](/README_进阶教程.md)

以下内容只针对Steam游戏版本，如需安卓版本教程请读原教程。你也可以将此教程当作参考。

作者：Kolyn090

教程游戏版本：v2.9.1

使用Windows x64

教程日期：5/15/2025

⚡ **警告**：模组修改可能带来未知风险，包括游戏不稳定、存档损坏、兼容性问题，甚至安全漏洞。切记备份游戏文件，并谨慎操作。
风险自担——如果出现问题，我无法承担责任。

### 步骤一
首先，你需要下载[AssetStudio](https://github.com/Perfare/AssetStudio/releases)。
我使用的是AssetStudio.net6.v0.16.47。这个版本要求你下载[.NET 6.0](https://dotnet.microsoft.com/en-us/download/dotnet/6.0)
到你的电脑里。选择符合你电脑配置的正确版本下载。


然后，下载[UABEA](https://github.com/nesrak1/UABEA/releases)。
我使用的是2024年11月份的版本（目前最新）。接着下载[Visual Studio 2022 IDE](https://visualstudio.microsoft.com/vs/)的Community版本。
下载安装完成后在电脑搜索栏输入“Visual Studio Installer”。在Installed界面你应该会看见Visual Studio
Community 2022。 点击Modify。在Individual components，输入“Visual C++”，你应该会看见一个叫C++ 2022 Redistributable Update的选项。
下载并安装它，然后关闭Visual Studio Installer并重启你的电脑。


最后你还需要下载[AddressablesTools](https://github.com/nesrak1/AddressablesTools/releases)。
我使用的是2025年4月的版本。不过要使用这个程序需要下载[.NET 8.0](https://github.com/nesrak1/AddressablesTools/releases)
到你的电脑里。下载方式同上。

### 步骤二
这一步需要你找到游戏的文件夹，也就是Steam存放游戏文件的位置。以下为示例：
```
Local Disk (C:)/Program Files (x86)/Steam/steamapps/common/Otherworld Legends
```
![game_folder](/images/game_folder.png)

如果你成功打开该文件夹，你应该会看见一个叫Otherworld Legends的可执行文件，这个就是可以打开玩的游戏了，不过我们的侧重点在
Otherworld Legends_Data这个文件夹。

打开Otherworld Legends_Data -> SteamingAssets -> aa -> StandaloneWindows64

这里汇集了我们可以更改替换的游戏资源。比如你想改游戏英雄皮肤。就打开
spriteassetgroup_assets_assets -> needdynamicloadresources -> spritereference。

![spritereference_unit_hero](/images/spritereference_unit_hero.png)

查找“unit_hero”，你会看见很多后缀为BUNDLE的文件。权虎鹰的内部命名为quanhuying，
银藏是seebee。我们可以用AssetStudio打开这些文件来确认哪个文件是属于哪位角色的。

### 步骤三
解压你下载好的AssetStudio，在解压文件里面找到一个叫AssetStudioGUI的可运行文件并打开它。

左上角点击File -> Load file，然后找到一个角色的BUNDLE文件。拿银藏举例，在spritereference里面搜索
unit_hero_seebee，第一个搜到的就是银藏的原始皮肤。在AssetStudio打开该文件。

![unit_hero_seebee](/images/unit_hero_seebee.png)

然后在工具栏里找到Asset List，你应该会看见银藏的作画文件（会在Preview里面显示）。我们这里只需要关注一个文件，
找到一个Type为Texture2D的文件，叫unit_hero_seebee。选择该文件，在工具栏找到Export -> Selected assets，
保存Texture2D到一个你记得住的地方。

![asset_studio_seebee](/images/asset_studio_seebee.png)

接下来你就可以展现一下你的画工了。推荐使用Aseprite编辑，当然你也可以使用像dotpict这样的免费软件。
保存你编辑好的unit_hero_seebee.png文件到一个你记得住的地方。记住编辑好的文件也应该是原来的后缀。

![aseprite_seebee_example](/images/aseprite_seebee_example.png)

### 步骤四
在这个步骤我们会使用UABEA来替换资源。解压下载好的UABEA，在其中找到UABEAvalonia可执行文件，打开它。
在小窗口左上角，点击File -> Open，找到在spritereference里面的unit_hero_seebee那个文件并打开。
然后你应该会看见一个弹出窗口提示。选择Memory就行。接着你会看见所有载入好的文件，还是一样，我们只关心
Type是Texture2D的那个文件。如果你看不见Type可以扩展一下你的窗口。

![uabea_assets_info](/images/uabea_assets_info.png)

![replace_texture](/images/replace_texture.png)

找到并选择该文件，在右边的工具栏，找到Plugins -> Edit texture -> Ok。 然后最下面有个Texture，
点击它旁边的Load，找到你编辑好的文件就可以替换它了。然后点击Save。在Assets Info窗口的上方工具栏File -> Save。
在UABEA窗口上方的工具栏File -> Save。这样就替换好了，放心关闭UABEA。不过我们还有一步要做。

### 步骤五
首先解压addrtool-example-windows。打开并找到Example这个可执行文件。记录它在你电脑里的位置，
比如
```
"C:\Users\kolynlin\Downloads\AddrTool\Example.exe"
```
接着找到一个叫catelog.json的文件。在我的电脑上，它的位置为
```
"C:\Program Files (x86)\Steam\steamapps\common\Otherworld Legends\Otherworld Legends_Data\StreamingAssets\aa\catalog.json"
```
确认该文件的位置并记录。

当你记录了这两个地方，在你电脑的搜索栏，输入Command Prompt并打开。
输入以下命令 (⚠️这是示例，不要忘了换成你记录的位置)：
```
"C:\Users\kolynlin\Downloads\AddrTool\Example.exe" patchcrc "C:\Program Files (x86)\Steam\steamapps\common\Otherworld Legends\Otherworld Legends_Data\StreamingAssets\aa\catalog.json"
```

![command_prompt](/images/command_prompt.png)

至此为止你应该就成功替换银藏的初始皮肤了。打开游戏并以确认。

![golden_ginzo](/images/golden_ginzo.png)

完成了基础教程，试试看这个[进阶教程](/README_进阶教程.md)吧！

---

由原教程作者咖喱猫扒饭授权。被授权bilibili账号：木瓜凝乳蛋白酶

如果你觉得这个教程对你有帮助，可以考虑给[原教程](https://b23.tv/IkX17vZ)三连和给此教程一个Star。
