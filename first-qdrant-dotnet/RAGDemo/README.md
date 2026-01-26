# RAG Demo - .NET 10 + Azure OpenAI + Qdrant + Local RERANK

这是一个完整的 RAG (Retrieval-Augmented Generation) 演示项目，集成了以下技术栈：

- **.NET 10** - 主程序框架
- **Azure OpenAI** - text-embedding-3-large 嵌入模型 + GPT-4o 聊天模型
- **Qdrant** - 向量数据库
- **Local RERANK** - 本地重排序服务 (bge-reranker-v2-m3)

## 功能特性

1. **文档分割** - 将长文档按段落分割成小片段
2. **向量嵌入** - 使用 Azure OpenAI 的 text-embedding-3-large 生成嵌入
3. **向量存储** - 将嵌入存储到 Qdrant 向量数据库
4. **语义搜索** - 基于查询向量在 Qdrant 中搜索相关片段
5. **重排序** - 使用本地 RERANK 服务优化搜索结果排序
6. **答案生成** - 使用 GPT-4o 基于检索到的上下文生成最终答案

## 环境要求

### 1. Azure OpenAI 配置
设置环境变量：
```bash
set AZURE_OPENAI_API_KEY=your_api_key_here
```

### 2. Qdrant 数据库
启动本地 Qdrant 实例：
```bash
docker run -p 6333:6333 -p 6334:6334 qdrant/qdrant
```

### 3. 本地 RERANK 服务
需要在 `http://localhost:5005` 运行 bge-reranker-v2-m3 ONNX 重排序服务。

服务应提供 `/rerank` 端点，接受以下格式的请求：
```json
{
  "Query": "用户查询",
  "Documents": ["文档1", "文档2", "..."]
}
```

返回格式：
```json
{
  "Results": [
    {"Index": 0, "Score": 0.95},
    {"Index": 1, "Score": 0.87}
  ]
}
```

## 运行程序

```bash
dotnet run
```

## 程序流程

1. **文档加载** - 读取 `AppData/doc.md` 文档
2. **文档分割** - 按段落分割文档
3. **嵌入生成** - 为每个片段生成向量嵌入
4. **向量存储** - 存储到 Qdrant 集合中
5. **查询处理** - 对用户查询生成嵌入并搜索
6. **结果重排序** - 使用 RERANK 服务优化结果
7. **答案生成** - 使用 GPT-4o 生成最终答案

## 示例查询

程序默认查询：`"哆啦A梦使用的3个秘密道具分别是什么?"`

基于提供的测试文档，程序应该能够准确识别并返回三个秘密道具：
- 复制斗篷
- 时间停止手表  
- 精神与时光屋便携版

## 项目结构

```
RAGDemo/
├── Program.cs              # 主程序
├── RAGDemo.csproj         # 项目文件
├── AppData/
│   └── doc.md             # 测试文档
├── RERANK/
│   ├── IRerankService.cs  # 重排序服务接口
│   └── LocalRerankService.cs # 本地重排序服务实现
└── README.md              # 说明文档
```

## 技术细节

- **向量维度**: 根据 text-embedding-3-large 模型自动确定
- **相似度计算**: 使用余弦相似度
- **搜索阈值**: 0.5 (可调整)
- **重排序数量**: 前5个结果
- **GPT 参数**: Temperature=0.1, MaxTokens=1000