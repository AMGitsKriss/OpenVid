namespace Database.Users
{
    public class UserRepository
    {
        private readonly UserDbContext _dbContext;

        public UserRepository(UserDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        /*public User GetUser(string name)
        {
            return _dbContext.GetUser(name);
        }*/
    }
}
