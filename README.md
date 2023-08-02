# FTP Client

用 C#.NET 开发的 FTP 客户端。功能实用，具有图形化界面，美观易用，操作方式符合直觉，使用体验舒适。


## 界面功能介绍

### FTP 客户端主窗体

该窗体是 FTP 客户端图形化界面的主体部分。

可调整大小、最大化或最小化，其内容布局会随时适应窗体的大小。

### 日志栏

位于窗体底部。

用来输出应答内容、消息、状态和错误信息。每行以“[时间] 类型：内容”的格式输出。

右键菜单中点击“清空”可以清空所有日志。

### 连接验证栏

位于窗体顶部。

输入 FTP 服务器的 IP 地址、用户名（为空，则采用匿名验证）和密码，并点击“连接”，即可连接到 FTP 服务器。

连接和验证的结果会输出到日志栏，连接成功后远程文件列表会显示根目录的内容。

### 远程文件列表

位于窗体左中部。

该列表显示的是当前工作目录的内容。

列表的表头分为三栏，名称、修改日期和大小（对目录来说，后两项都显示“<目录>”）。

列表中默认的排序方法是，先按“目录在先、文件在后”，后按名称的字典序升序。

鼠标左键双击一个目录时，转到该目录下；双击一个文件时，下载该文件。

选中若干文件或目录后（支持多选），右键单击，在弹出的右键菜单中，有下载、重命名和删除操作，可以批量下载文件、批量删除文件和目录（删除时，会弹出“确认永久删除这 XX 个文件、XX 个目录及目录中的所有子项”的警告消息框；删除一个目录时，其所有子文件和子目录都被级联删除）。

上方的文本框显示当前工作目录的路径，也接受手动键入一个有效的路径（不规范的路径会被自动改为规范格式；若路径无效，会输出错误信息并回滚）；“↑”按钮用来返回上级目录；“刷新”按钮用来重新获取工作目录的文件列表。

### “上传文件”“新建目录”按钮

位于窗体右中部的上方。

点击“上传文件”按钮，弹出选择文件对话框，可批量选择一些文件，上传到当前工作目录。

点击“新建目录”按钮，在弹出输入框中输入一个有效的目录名，来新建一个目录。

若文件重复，则弹出“文件已存在，确认覆盖”的警告消息框，选择“确定”才会继续上传。

### 本地默认路径设置栏

位于窗体右中部的上方。

双击文本框，弹出选择目录对话框，来设置本地默认路径。

在文本框中键入一个有效路径，也可以设置本地默认路径（不规范的路径会被自动改为规范格式；若路径无效，会输出错误信息并清空文本框）。

点击“打开”，在文件资源管理器中打开对应的本地目录。

在本地默认路径未被设置的情况下，下载文件会弹出错误消息框，无法下载。

### 传输列表

位于窗体右中部的主体部分。

该列表显示的是文件传输任务。

列表的表头分为两栏，文件名和状态。

传输类型分为上传和下载，状态类型分为进行中、暂停、失败、已完成和已取消，在状态栏中还会显示进行中任务的百分比进度（如 12.34%）。

选中若干传输任务后，右键单击，在弹出的右键菜单中，会显示这些传输任务所共有的可进行操作（所有操作包括暂停、继续、重试和取消）。

右键单击列表任意位置，弹出的右键菜单中都有清空非活跃任务的操作。

下载任务遇到本地路径中重名的文件，会自动在文件名后加小括号重命名；继续或重试的任务，若之前已经下载了一部分，会进行断点续传。
