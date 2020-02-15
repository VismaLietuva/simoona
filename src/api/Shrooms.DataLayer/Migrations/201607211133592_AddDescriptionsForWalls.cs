namespace Shrooms.DataLayer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddDescriptionsForWalls : DbMigration
    {
        public override void Up()
        {
            Sql(@"
                UPDATE [dbo].[Walls]
                SET[Description] = 'Do not let the name fool you – it‘s not a „No Boys Allowed“ wall. We love our girls for being so fantastic. Guys can sometimes learn to be so nice and polite. This wall is exactly what it is. Only courteous and fair-spoken discussions and topics. In other words – sugar, spice and everything nice.'   
                Where [Name] = 'Girls'

                UPDATE [dbo].[Walls]
                SET[Description] = 'This is the wall for most necessary and vital information for Visma employees. No #blevyzgos, no #spam. We’d basically post something there if the building was on fire or when the food arrives. Join this wall if you’re constantly in “do not disturb” mode.'   
                Where [Name] = 'GrumpyCat'

                UPDATE [dbo].[Walls]
                SET[Description] = 'If Facebook, Twitter, Delfi and all other sources of information is not enough for you – join the Žvejai Wall. That is the place, where all unspecified and, let‘s say, random information from your fellow colleagues goes. Meaning an information, that you can definitely live without, but it‘s kind of „Cool story bro“ stuff.'   
                Where [Name] = 'Žvejai'

                UPDATE [dbo].[Walls]
                SET[Description] = 'A place where you can demonstrate your beautiful geekiness and take part in active discussions on latest and hottest geeky topics. Analyze theories of Game of Thrones, suggest new gadgets for techies club, demonstrate your new board game or just share a very great book. Whatever comes to your geeky mind.'   
                Where [Name] = 'Geeks'
                ");
        }
        
        public override void Down()
        {
        }
    }
}
