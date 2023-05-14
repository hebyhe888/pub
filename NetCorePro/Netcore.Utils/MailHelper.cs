using Microsoft.Extensions.Configuration;
using MimeKit;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Mail;
using System.Threading.Tasks;
using SmtpClient = MailKit.Net.Smtp.SmtpClient;
using Microsoft.Extensions.Configuration.Json;

namespace Utils
{
    public class MailHelper
    {
        public static IConfiguration Configuration { get; set; }
        static MailHelper()
        {
            string Path = "appsettings.json";
            Configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .Add(new JsonConfigurationSource
                {
                    Path = Path,
                    Optional = false,
                    ReloadOnChange = true
                })
                .Build();// 这样的话，可以直接读目录里的json文件，而不是 bin 文件夹下的，所以不用修改复制属性
        }
        /// <summary>
        /// 发送邮箱
        /// </summary>
        public static string sender { get; set; }
        /// <summary>
        /// 邮箱密码
        /// </summary>
        public static string password { get; set; }
        /// <summary>
        /// 邮箱主机
        /// </summary>
        public static string host { get; set; } = "smtp.mxhichina.com";
        /// <summary>
        /// 邮箱端口
        /// </summary>
        public static int port { get; set; } = 25;
        /// <summary>
        /// 发送邮件
        /// </summary>
        /// <param name="Name">发件人名字</param>
        /// <param name="receive">接收邮箱</param>
        /// <param name="subject">邮件主题</param>
        /// <param name="body">邮件内容</param>
        /// <returns></returns>
        public async static Task<bool> SendMailAsync(string Name,string subject, string body, List<string> receive=null, List<string> cc = null,List<string> bcc=null)
        {
            try
            {
                sender = Configuration.GetSection("Email:Smtp:Username").Value;
                password = Utils.DESEncrypt.Decrypt(Configuration.GetSection("Email:Smtp:Password").Value);
                host = Configuration.GetSection("Email:Smtp:Host").Value;
                port = Convert.ToInt32(Configuration.GetSection("Email:Smtp:Port").Value);
                var receivers = Configuration.GetSection("Email:Smtp:Receiver").Value;
                //MimeMessage代表一封电子邮件的对象
                var message = new MimeMessage();
                //添加发件人地址 Name 发件人名字 sender 发件人邮箱
                message.From.Add(new MailboxAddress(Name, sender));
                //添加收件人地址
                if (receive == null)
                {
                    receive = new List<string>();
                    foreach(var rc in receivers.Split(';'))
                    {
                        receive.Add(rc);
                    }
                }
                if (receive.Count > 0)
                {
                    receive.ForEach(rc =>
                    {
                        message.To.Add(new MailboxAddress("", rc));
                    });
                }
                if(cc != null && cc.Count>0)
                {
                    cc.ForEach(tempcc =>
                    {
                        message.Cc.Add(new MailboxAddress("", tempcc));
                    });
                }
                if (bcc != null && bcc.Count > 0)
                {
                    bcc.ForEach(tempcc =>
                    {
                        message.Bcc.Add(new MailboxAddress("", tempcc));
                    });
                }
                //设置邮件主题信息
                message.Subject = subject;
                //设置邮件内容
                var bodyBuilder = new BodyBuilder() { HtmlBody = body };
                message.Body = bodyBuilder.ToMessageBody();
                using (var client = new SmtpClient())
                {
                    client.ServerCertificateValidationCallback = (s, c, h, e) => true;
                    // Note: since we don't have an OAuth2 token, disable  
                    // the XOAUTH2 authentication mechanism.  
                    client.AuthenticationMechanisms.Remove("XOAUTH2");
                    client.CheckCertificateRevocation = false;
                    //client.SslProtocols = System.Security.Authentication.SslProtocols.Tls12;
                    client.Connect(host, port, MailKit.Security.SecureSocketOptions.Auto);
                    // Note: only needed if the SMTP server requires authentication
                    client.Authenticate(sender, password);
                    await client.SendAsync(message);
                    client.Disconnect(true);
                    return true;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
