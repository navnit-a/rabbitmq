using System.Data.SqlClient;
using System.Threading.Tasks;
using Common;

namespace WorkerQueue_Consumer
{
    public class SqlStuff
    {
        public async Task<int> SaveStuff(Payment payment)
        {
            using (var con =
                new SqlConnection(
                    "Initial Catalog=Rabbit;Data Source=.;User Id=sa;Password=nmqapass;Connection Timeout=30;Min Pool Size=20; Max Pool Size=200;")
            )
            {
                con.Open();
                using (var command = new SqlCommand(
                    "INSERT INTO Payment VALUES(@name)", con))
                {
                    command.Parameters.Add(new SqlParameter("name", payment.Name));
                    await command.ExecuteNonQueryAsync();
                }
            }
            return 0;
        }
    }
}