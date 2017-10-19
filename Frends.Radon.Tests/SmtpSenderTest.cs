using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using System.Net.Mail;
using nDumbster.smtp;

namespace Frends.Radon.Tests
{
    [TestFixture]
    public class SmtpSenderTest
    {
        SimpleSmtpServer smtpServer;

        [SetUp]
        public void Setup()
        {
            smtpServer = SimpleSmtpServer.Start();
        }

        [TearDown]
        public void TearDown()
        {
            smtpServer.Stop();
        }

        [Ignore("Hangs in 'smtpServer.ReceivedEmailCount' for no reason in release build.")]
        [Test]
        public void SendingMailShouldWork()
        {
            var client = new SmtpSender("localhost", 25, true, 1000, false)
                                    {
                                        Sender = new MailAddress("dr.evil@evilcorp.com", "Dr Evil"),
                                        Recipients = new List<MailAddress> { new MailAddress("jouko.rules@totally.com", "Jouko Pouko") }
                                    };

            const string msgBody = "<html><head /><body><table><tbody><tr><td>Secret Plans</td></tr><tr><td>Ransom 100 billion dollars, btw you rule!</td></tr></tbody></table></body></html>";
            const string msgSubject = "Plans of World destruction";

            client.SendReport(msgBody, msgSubject);

            Assert.AreEqual(1, smtpServer.ReceivedEmailCount, "1 mails sent");
            SmtpMessage mail = smtpServer.ReceivedEmail[0];
            Assert.AreEqual("jouko.rules@totally.com", mail.Headers["To"], "Receiver");
            Assert.AreEqual("\"Dr Evil\" <dr.evil@evilcorp.com>", mail.Headers["From"], "Sender");
            Assert.AreEqual(msgSubject, mail.Headers["Subject"], "Subject");

            var body = ParseMessageBody(mail.Body);
            Assert.That(body, Is.StringContaining(msgBody));
        }

        [Test]
        public void EmptyPortNumberShouldSetPortToSmtpDefaultPort()
        {
            SmtpSender client = new SmtpSender("localhost", 0, true, 1000, false);
            Assert.AreEqual(25, client.SmtpClient.Port, "Port number should be default SMTP port: 25");
        }

        [Ignore("Hangs in 'smtpServer.ReceivedEmailCount' for no reason in release build.")]
        [Test]
        public void EmptyUserpasswordShouldWork()
        {
            var client = new SmtpSender("localhost", 25, false, 1000, "foo", null, useSsl:false)
                             {
                                 Recipients = new List<MailAddress> {new MailAddress("user@foo.com")},
                                 Sender = new MailAddress("sender@foo.com")
                             };

            client.SendReport("<Perfect/>", "Foo");

            Assert.That(smtpServer.ReceivedEmailCount, Is.EqualTo(1));

            var body = ParseMessageBody(smtpServer.ReceivedEmail.Single().Body);
            Assert.That(body, Is.StringContaining("<Perfect/>"));            
        }

        public static string ParseMessageBody(string body)
        {
            return body.Replace("=0D=0A", "\n").Replace("=\r\n", "");
        }
    }
}
