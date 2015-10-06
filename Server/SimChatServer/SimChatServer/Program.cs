using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


/// <summary>
/// 
/// </summary>
namespace SimChatServer
{

    /// <summary>
    /// 
    /// </summary>
    class Program
    {

        /// <summary>
        /// Mains the specified arguments.
        /// </summary>
        /// <param name="args">The arguments.</param>
        static void Main(string[] args)
        {

            Server server = new Server();
            server.Listen();
        }
    }
}
