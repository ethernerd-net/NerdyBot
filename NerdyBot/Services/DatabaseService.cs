using SQLite;

namespace NerdyBot.Services
{
  public class DatabaseService
  {
    private const string DBNAME = "db.db"; 
    private SQLiteConnection db;

    public SQLiteConnection Database
    {
      get
      {
        if ( this.db == null )
          this.db = new SQLiteConnection( DBNAME );
        return this.db;
      }
    }
  }
}
