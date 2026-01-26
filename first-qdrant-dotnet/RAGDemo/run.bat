@echo off
echo 检查环境变量...
if "%AZURE_OPENAI_API_KEY%"=="" (
    echo 错误: 请设置 AZURE_OPENAI_API_KEY 环境变量
    echo 使用命令: set AZURE_OPENAI_API_KEY=your_api_key_here
    pause
    exit /b 1
)

echo 启动 RAG Demo...
dotnet run

pause