﻿using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace FIT5032_Week08A.Utils
{
    public class EmailSender
    {
        private const String API_KEY = "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";

        public void SendSingleMail(String toEmail, String subject, String contents)
        {
            var client = new SendGridClient(API_KEY);
            var from = new EmailAddress("noreply@localhost.com", "Be Our Guest");
            var to = new EmailAddress(toEmail, "");
            var plainTextContent = contents;
            var htmlContent = "<div>" + contents + "</div>";
            var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);
            var pdfBytes = (new NReco.PdfGenerator.HtmlToPdfConverter()).GeneratePdf(htmlContent);
            //var bytes = File.ReadAllBytes(pdfBytes);
            var file = Convert.ToBase64String(pdfBytes);
            msg.AddAttachment("Confimation.pdf", file);
            var response = client.SendEmailAsync(msg);
        }

    }
}