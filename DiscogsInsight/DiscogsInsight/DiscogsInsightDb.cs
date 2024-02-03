using DiscogsInsight.Models;
using SQLite;

namespace DiscogsInsight
{
    public class DiscogsInsightDb


    {
        SQLiteAsyncConnection Database;

        public DiscogsInsightDb()
        {
        }

        async Task Init()
        {
            if (Database is not null)
                return;

            Database = new SQLiteAsyncConnection(Constants.DatabasePath, Constants.Flags);
            var result = await Database.CreateTableAsync<Artist>();
        }

        public async Task<List<Artist>> GetArtistsAsync()
        {
            await Init();
            return await Database.Table<Artist>().ToListAsync();
        }

        //public async Task<List<TodoItem>> GetItemsNotDoneAsync()
        //{
        //    await Init();
        //    return await Database.Table<TodoItem>().Where(t => t.Done).ToListAsync();

        //    // SQL queries are also possible
        //    //return await Database.QueryAsync<TodoItem>("SELECT * FROM [TodoItem] WHERE [Done] = 0");
        //}

        //public async Task<T> GetEntityAsync<T>(T item) where T : IDatabaseEntity
        //{
        //    await Init();
        //    return await Database.GetTableInfoAsync<T>();
        //}

        public async Task<int> SaveItemAsync<T>(T item) where T : IDatabaseEntity
        {
            try
            {
                await Init();
                if (item.Id != 0)
                    return await Database.UpdateAsync(item);

                return await Database.InsertAsync(item);

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<int> SaveArtistsAsync(Artist item)
        {
            await Init();
            if (item.Id != 0)
                return await Database.UpdateAsync(item);

            return await Database.InsertAsync(item);
        }

        //public async Task<int> DeleteItemAsync(TodoItem item)
        //{
        //    await Init();
        //    return await Database.DeleteAsync(item);
        //}
    }
}

