namespace Shrooms.DataLayer.Migrations
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity.Infrastructure.Annotations;
    using System.Data.Entity.Migrations;
    
    public partial class AddKudosWelcomeColumns : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Organizations", "KudosWelcomeAmount", c => c.Int(nullable: false,
                annotations: new Dictionary<string, AnnotationValues>
                {
                    { 
                        "SqlDefaultValue",
                        new AnnotationValues(oldValue: null, newValue: "0")
                    },
                }));
            AddColumn("dbo.Organizations", "KudosWelcomeEnabled", c => c.Boolean(nullable: false,
                annotations: new Dictionary<string, AnnotationValues>
                {
                    { 
                        "SqlDefaultValue",
                        new AnnotationValues(oldValue: null, newValue: "0")
                    },
                }));
            AddColumn("dbo.Organizations", "KudosWelcomeComment", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Organizations", "KudosWelcomeComment");
            DropColumn("dbo.Organizations", "KudosWelcomeEnabled",
                removedAnnotations: new Dictionary<string, object>
                {
                    { "SqlDefaultValue", "0" },
                });
            DropColumn("dbo.Organizations", "KudosWelcomeAmount",
                removedAnnotations: new Dictionary<string, object>
                {
                    { "SqlDefaultValue", "0" },
                });
        }
    }
}
