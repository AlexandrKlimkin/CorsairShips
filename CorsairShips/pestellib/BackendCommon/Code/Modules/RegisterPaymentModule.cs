using PestelLib.ServerShared;
using S;

namespace ServerLib.Modules
{
    public class RegisterPaymentModule : IModule
    {
        public ServerResponse ProcessCommand(ServerRequest request)
        {
            return new ServerResponse { ResponseCode = ResponseCode.OK };
            /*
            var r = request.Request.RegisterPayment;

            try
            {
                using (var connection = DatabaseUtils.CreateAndOpenConnection())
                {
                    using (var command = new MySqlCommand(
                                "INSERT INTO transactions VALUES(0, @userId, @networkType, @userName, @paymentAmountOut, @paymentAmountLocal, @paymentCurrency, NOW());",
                                connection))
                    {
                        command.Parameters.AddWithValue("@userId", request.Request.UserId);
                        command.Parameters.AddWithValue("@networkType", request.Request.NetworkId);
                        command.Parameters.AddWithValue("@userName", r.userName);
                        command.Parameters.AddWithValue("@paymentAmountOut", r.paymentAmountOut);
                        command.Parameters.AddWithValue("@paymentAmountLocal", r.paymentAmountLocal);
                        command.Parameters.AddWithValue("@paymentCurrency", r.paymentCurrency);
                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (MySqlException ex)
            {
                Debug.Print(ex.Message);
                throw;
            }

            return new ServerResponse {ResponseCode = ResponseCode.OK};
            */
        }
    }
}
