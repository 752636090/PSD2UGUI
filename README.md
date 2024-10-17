# 工具特色
此工具仅减少PS切图和在Unity引擎中引用图片、对齐图片的工作量。相比其它同类工具，在不需要PS插件且不需要付费PSD工具库的同时最小化对UI美术的影响。

# 重要说明
1. 只自动添加Image组件，其余组件、辅助物体都要自己手动增加删除修改，自适应锚点也要自己改，这些手动改的地方不需要在下一次生成UI之后重新操作
2. 在编辑器环境中，UI图片组件引用的图片的SprieMode都要求是Single
3. 自动添加的图片的raycastTarget为false
4. 可以改UI内物体名，但不能改生成的图片名

# 重要引用插件
1. Odin（需自己导入）
2. 2D PSD Importer（Unity官方包，项目中已导入）

# 设置方式
ProjectSettings-PSD2UGUI，里面有很多Tooltip，遇到不懂的设置可以试试看一下Tooptip

# 美术制作规范
1. 同一PSD内图层和图层组不能重名。
2. 遇到下列情况，就要把该图层做成智能对象，做成智能对象以后千万不要继续修改，若要修改则先转回图层。
    (1) 图层“不透明度”或者“填充”任意一个不是100%，不包括画笔的不透明度
    (2) 图层用了“混合选项”（特效）
    (3) 用了图层蒙版
3. 图层名称后面可以加标签，例如某个通用UI子界面的图层组名为“SkillSlot_1[Ref:SkillSlot]”，冒号后的多个参数用竖线“|”隔开，以下是所有标签：
    Ref: 通用UI子界面，工具遇到这个标签就会直接引用现有文件，注意要先生成子界面再生成引用它的界面。
        参数1：引用的文件名称。
    Slice: 九宫格切图
        参数1（可省略，默认为0）："0"表示横竖都切，"1"表示只竖着拉伸，“2”表示只横着拉伸.
        例如：BgFrame[Slice]、BgFrame[Slice:0]。
    Ignore: 不导出（隐藏图层也不会导出），通常用于滚动列表项这类程序动态显示的部分。也可以不加该标签，首次导出后可以人工删除。

# 导出流程
打开Unity上方菜单Tools-WindowEditor-PSD2UGUI，直接看着操作即可，有问题会弹窗说明