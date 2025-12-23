using Sundial;
using SundialExercises;
using TimeCrontab;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 注册 Sundial 作业并配置触发器
builder.Services.AddSchedule(options =>
{ 
    options.AddJob<WeChatJob>(Triggers.PeriodSeconds(5)); // Runs every 5 seconds

    //.NET Cron 表达式解析库 TimeCrontab：https://gitee.com/dotnetchina/TimeCrontab
    var cronttabl = Crontab.DailyAt(3);// 每天第 3 小时正（点）
    var cronttab2 = Crontab.WeeklyAt(DayOfWeek.Monday, 9, 30);
    var cronttab3 = Crontab.WeeklyAt("WED");// SUN（星期天），MON，TUE，WED，THU，FRI，SAT
    var crontab4 = Crontab.YearlyAt(3); // 每年第 3，5，6 月 1 日零点正
    //options.AddJob<CustomJob>(Triggers.Cron(crontab.ToString()));
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
