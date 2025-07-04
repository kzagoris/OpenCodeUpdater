namespace OpenCodeUpdater.Errors;

public record HttpError(string Message, Exception? Exception = null);
public record FileError(string Message, Exception? Exception = null);
public record ValidationError(string Message);
public record GeneralError(string Message, Exception? Exception = null);
public record Success();