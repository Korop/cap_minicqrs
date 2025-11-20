namespace Cap.MiniCqrs;

public interface ICommand { }

public interface ICommand<TResponse> : ICommand { }
