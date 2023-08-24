
using System;
using System.Diagnostics;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

using iTin.Core.ComponentModel;
using iTin.Core.ComponentModel.Results;
using iTin.Core.Helpers;

using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace iTin.Mail.Smtp.Net;

/// <summary>
/// class which encapsules the behavior of send mail.
/// </summary>
public class SmtpMail
{
    #region public constants

    /// <summary>
    /// <strong>gmail</strong> smtp host
    /// </summary>
    public const string GmailSmtpHost = "smtp.gmail.com";

    /// <summary>
    /// <strong>ethereal</strong> smtp host
    /// </summary>
    public const string EtherealSmtpHost = "smtp.ethereal.email";

    /// <summary>
    /// <strong>mailtrap</strong> smtp host
    /// </summary>
    public const string MailtrapSmtpHost = "smtp.mailtrap.io";

    #endregion

    #region private readonly fields

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private readonly SmtpMailSettings _settings;

    #endregion

    #region constructor/s

    /// <summary>
    /// Initializes a new instance of the <see cref="Mail"/> class.
    /// </summary>
    /// <param name="settings">The settings.</param>
    public SmtpMail(SmtpMailSettings settings)
    {
        _settings = settings;
    }

    #endregion

    #region public methods

    /// <summary>
    /// Sends mail with specified credential synchronously.
    /// </summary>
    /// <param name="message">Message to send.</param>
    /// <returns>
    /// <para>
    /// A <see cref="BooleanResult"/> which implements the <see cref="IResult"/> interface reference that contains the result of the operation, to check if the operation is correct, the <b>Success</b>
    /// property will be <b>true</b> and the <b>Value</b> property will contain the value; Otherwise, the the <b>Success</b> property
    /// will be false and the <b>Errors</b> property will contain the errors associated with the operation, if they have been filled in.
    /// </para>
    /// <para>
    /// The type of the return value is <see cref="bool"/>, which contains the operation result
    /// </para>
    /// </returns>
    public IResult SendMail(MimeMessage message)
    {
        SentinelHelper.ArgumentNull(message, nameof(message));

        var hostIsEmpty = string.IsNullOrEmpty(_settings.Credential.Host);
        if (hostIsEmpty)
        {
            return BooleanResult.CreateErrorResult("Host can not be empty");
        }

        using var smtp = new SmtpClient();

        var canUseSecureSocket = CanUseSecureSocket(_settings.Credential.Host.Trim());
        if (canUseSecureSocket)
        {
            smtp.Connect(
                _settings.Credential.Host,
                _settings.Credential.Port,
                SecureSocketOptions.StartTls);
        }
        else
        {
            smtp.Connect(
                _settings.Credential.Host,
                _settings.Credential.Port,
                _settings.Credential.UseSsl);
        }

        var userNameIsEmpty = string.IsNullOrEmpty(_settings.Credential.UserName);
        if (userNameIsEmpty)
        {
            return BooleanResult.CreateErrorResult("UserName can not be empty");
        }

        var credential = new NetworkCredential(
            _settings.Credential.UserName,
            _settings.Credential.Password,
            _settings.Credential.Domain);
        smtp.Authenticate(credential);

        try
        {
            smtp.Send(message);
            smtp.Disconnect(true);

            return BooleanResult.SuccessResult;
        }
        catch (Exception e)
        {
            return BooleanResult.FromException(e);
        }
    }

    #endregion

    #region public async methods

    /// <summary>
    /// Sends mail with specified credential synchronously.
    /// </summary>
    /// <param name="message">Message to send.</param>
    /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
    /// <returns>
    /// <para>
    /// A <see cref="BooleanResult"/> which implements the <see cref="IResult"/> interface reference that contains the result of the operation, to check if the operation is correct, the <b>Success</b>
    /// property will be <b>true</b> and the <b>Value</b> property will contain the value; Otherwise, the the <b>Success</b> property
    /// will be false and the <b>Errors</b> property will contain the errors associated with the operation, if they have been filled in.
    /// </para>
    /// <para>
    /// The type of the return value is <see cref="bool"/>, which contains the operation result
    /// </para>
    /// </returns>
    public async Task<IResult> SendMailAsync(MimeMessage message, CancellationToken cancellationToken = default)
    {
        SentinelHelper.ArgumentNull(message, nameof(message));

        var hostIsEmpty = string.IsNullOrEmpty(_settings.Credential.Host);
        if (hostIsEmpty)
        {
            return BooleanResult.CreateErrorResult("Host can not be empty");
        }

        using var smtp = new SmtpClient();

        var canUseSecureSocket = CanUseSecureSocket(_settings.Credential.Host.Trim());
        if (canUseSecureSocket)
        {
            await smtp.ConnectAsync(
                _settings.Credential.Host,
                _settings.Credential.Port,
                SecureSocketOptions.StartTls, 
                cancellationToken);
        }
        else
        {
            await smtp.ConnectAsync(
                _settings.Credential.Host,
                _settings.Credential.Port,
                _settings.Credential.UseSsl,
                cancellationToken);
        }

        var userNameIsEmpty = string.IsNullOrEmpty(_settings.Credential.UserName);
        if (userNameIsEmpty)
        {
            return BooleanResult.CreateErrorResult("UserName can not be empty");
        }

        var credential = new NetworkCredential(
            _settings.Credential.UserName, 
            _settings.Credential.Password, 
            _settings.Credential.Domain);
        await smtp.AuthenticateAsync(credential, cancellationToken);

        try
        {
            await smtp.SendAsync(message, cancellationToken);
            await smtp.DisconnectAsync(true, cancellationToken);

            return BooleanResult.SuccessResult;
        }
        catch (Exception e)
        {
            return BooleanResult.FromException(e);
        }
    }

    #endregion

    #region private static methods

    private static bool CanUseSecureSocket(string host) =>
        host.Contains("smtp.mailtrap") ||
        host.Contains("smtp.ethereal");

    #endregion
}
