namespace Shrooms.DataLayer.Migrations
{
    using System.Data.Entity.Migrations;
    
    public partial class ChangedPropertyOfPostAndCommentFromPictureIdToImageCollection : DbMigration
    {
        public override void Up()
        {
            RenameColumn(name: "PictureId", table: "dbo.Posts", newName: "Images");
            RenameColumn(name: "PictureId", table: "dbo.Comments", newName: "Images");
        }
        
        public override void Down()
        {
            RenameColumn(name: "Images", table: "dbo.Posts", newName: "PictureId");
            RenameColumn(name: "Images", table: "dbo.Comments", newName: "PictureId");
        }
    }
}
