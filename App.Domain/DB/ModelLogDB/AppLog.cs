using App.Domain.Bases;
using System;
using System.Collections.Generic;

namespace App.Domain.DB.ModelLogDB;

[EntityAttribute(DBName = "LogDB")]
public partial class AppLog : BaseEntity
{
    public string? Message { get; set; }

    public string? MessageTemplate { get; set; }

    public string? Level { get; set; }

    public DateTime? TimeStamp { get; set; }

    public string? Exception { get; set; }

    public string? Properties { get; set; }

}
