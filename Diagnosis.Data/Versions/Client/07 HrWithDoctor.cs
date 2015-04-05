using Diagnosis.Models;
using FluentMigrator;
using PasswordHash;
using NHibernate.Linq;
using System.Linq;
using System;

namespace Diagnosis.Data.Versions.Client
{
    // this in initial-client.sql 
    //[Migration(201412191200)]
    //public class HrWithDoctor : Migration
    //{
    //    private const string Names.FK.Hr_Doctor = "Names.FK.Hr_Doctor";
    //    private const string CreatedAt = "CreatedAt";
    //    private const string Ord = "Ord";
    //    private const string DoctorId = Names.Id.Doctor;

    //    public override void Up()
    //    {
    //        Alter.Table(Names.HealthRecordTbl)
    //            .AddColumn(CreatedAt).AsDateTime().NotNullable().WithDefault(SystemMethods.CurrentDateTime)
    //            .AddColumn(Ord).AsByte().NotNullable().WithDefaultValue(0)
    //            .AddColumn(DoctorId).AsGuid().NotNullable().WithDefaultValue(Guid.Empty);

    //        using (var s = NHibernateHelper.Default.OpenSession())
    //        {
    //            //s.Query<HealthRecord>().Where(hr => hr.Course != null).ForEach(hr => hr.Doctor = hr.Course.LeadDoctor);
    //            //s.Query<HealthRecord>().Where(hr => hr.Appointment != null).ForEach(hr => hr.Doctor = hr.Appointment.Doctor);
    //            // старые записи пациента остаются без доктора 

    //            //var doc = s.Query<Doctor>().First();
    //            //s.Query<HealthRecord>().Where(hr => hr.Patient != null).ForEach(hr => hr.Doctor = doc);
    //        }

    //        //Create.ForeignKey(Names.FK.Hr_Doctor).FromTable(Names.HealthRecordTbl)
    //        //   .ForeignColumn(DoctorId)
    //        //   .ToTable(Names.DoctorTbl)
    //        //   .PrimaryColumn("Id");
    //    }

    //    public override void Down()
    //    {
    //        //  Delete.ForeignKey(Names.FK.Hr_Doctor).OnTable(Names.HealthRecordTbl);
    //        Delete.Column(CreatedAt).FromTable(Names.HealthRecordTbl);
    //        Delete.Column(DoctorId).FromTable(Names.HealthRecordTbl);
    //        Delete.Column(Ord).FromTable(Names.HealthRecordTbl);
    //    }
    //}
}