# 工作路径功能测试说明

## 功能概述

已成功为 Claude Code 模型配置管理器添加工作路径选择功能。新增功能包括：

1. **工作路径选择界面**：在配置名称上方添加了工作路径显示标签和选择按钮
2. **路径持久化**：选择的工作路径会保存到 `appsettings.json` 文件中
3. **启动集成**：启动 Claude Code 时会先切换到工作路径

## 新增文件和修改

### 修改的文件

1. **MainWindow.xaml**：
   - 添加了工作路径选择区域的 UI 控件
   - 调整了现有控件的 Grid.Row 索引

2. **MainWindow.xaml.cs**：
   - 添加了 `_workingPath` 字段用于存储工作路径
   - 添加了 `BrowseWorkingPathButton_Click` 事件处理方法
   - 添加了 `LoadAppSettings()` 和 `SaveAppSettings()` 方法
   - 修改了启动按钮事件，传递工作路径参数

3. **BusinessLogic/SettingsManager.cs**：
   - 修改了 `LaunchClaudeWithProfile` 和 `LaunchClaudeDirectly` 方法
   - 添加了 `workingPath` 可选参数
   - 在启动前添加了 `cd` 命令

4. **ClaudeCodeLLMConfigManager.csproj**：
   - 添加了 System.Windows.Forms 引用

### 新增配置文件

- **appsettings.json**：运行时自动创建，用于存储工作路径设置

## 功能测试步骤

### 基本功能测试

1. **启动应用程序**：
   ```bash
   dotnet run --project ClaudeCodeLLMConfigManager.csproj
   ```

2. **验证UI显示**：
   - 确认"工作路径"标签显示在配置名称上方
   - 确认显示"未选择工作路径..."的标签
   - 确认"选择路径..."按钮存在

3. **测试路径选择**：
   - 点击"选择路径..."按钮
   - 在弹出的文件夹对话框中选择一个工作目录
   - 确认路径标签更新为选择的路径

4. **验证配置持久化**：
   - 关闭应用程序
   - 重新启动应用程序
   - 确认之前选择的工作路径仍然显示

5. **测试启动功能**：
   - 创建一个测试配置文件
   - 点击"根据配置文件启动Claude code"按钮
   - 确认 PowerShell 启动并在正确的工作目录下
   - 测试"直接启动 Claude code"按钮

### 启动命令验证

当选择了工作路径（如 `C:\Work`）和环境变量配置时，生成的 PowerShell 命令如下：

```powershell
cd 'C:\Work'; $Env:API_KEY='your-api-key'; $Env:MODEL='claude-3'; claude
```

## 错误处理

1. **路径访问权限**：如果选择无权限访问的路径，会在启动时给出错误提示
2. **配置文件损坏**：appsettings.json 文件损坏时会优雅降级，使用默认设置
3. **路径不存在**：如果选择的路径被删除，启动时会显示相应错误信息

## 注意事项

1. 工作路径是全局设置，不与特定配置文件关联
2. 如果未选择工作路径，启动行为与原版本完全相同
3. 路径中的特殊字符会被正确处理，使用单引号包裹
4. 应用程序会在当前目录下创建 `appsettings.json` 文件存储工作路径

## 兼容性

- 与现有功能完全兼容
- 不会影响现有配置文件的处理
- 向后兼容，没有选择工作路径时行为不变