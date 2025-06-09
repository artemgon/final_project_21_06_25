using BookLibrary.ApplicationServices.Contracts;
using BookLibrary.ApplicationServices.Implementations;
using BookLibrary.DataAccess.Contracts;
using DataAccess.Database;
using BookLibrary.DataAccess.Implementations;
using ViewModels;
using ViewModels.AuthorManagement; 
using ViewModels.BookManagement; 
using ViewModels.GenreManagement;   
using ViewModels.WishlistManagement;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;


namespace BookLibrary.WPF
{
    public partial class App : Application
    {
        private ServiceProvider _serviceProvider;
        private IConfiguration _configuration;

        public App()
        {
            // 1. Build Configuration (from appsettings.json)
            _configuration = new ConfigurationBuilder()
                .SetBasePath(System.IO.Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            // 2. Configure Services for Dependency Injection
            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);
            _serviceProvider = serviceCollection.BuildServiceProvider();
        }

        private void ConfigureServices(IServiceCollection services)
        {
            // Register DbConnectionFactory with the connection string
            // AddSingleton ensures only one instance of DbConnectionFactory is created
            services.AddSingleton<DbConnectionFactory>(
                new DbConnectionFactory(_configuration.GetConnectionString("BookLibraryDB")));

            // Register Repositories (Data Access Layer)
            // AddTransient means a new instance is created each time it's requested
            services.AddTransient<IBookRepository, BookRepository>();
            services.AddTransient<IAuthorRepository, AuthorRepository>();
            services.AddTransient<IGenreRepository, GenreRepository>();
            services.AddTransient<IWishlistRepository, WishlistRepository>();

            // Register Application Services (Business Logic Layer)
            // AddTransient means a new instance is created each time it's requested
            services.AddTransient<IBookService, BookService>();
            services.AddTransient<IAuthorService, AuthorService>();
            services.AddTransient<IGenreService, GenreService>();
            services.AddTransient<IWishlistService, WishlistService>();

            // Register ViewModels (Presentation Layer)
            // ViewModels are often Transient as they manage state for a specific view instance
            services.AddTransient<MainViewModel>();
            services.AddTransient<BookListViewModel>();
            services.AddTransient<BookDetailViewModel>();     // Will be created soon
            services.AddTransient<AuthorManagerViewModel>(); // Will be created soon
            services.AddTransient<GenreManagerViewModel>();   // Will be created soon
            services.AddTransient<WishlistManagerViewModel>();// Will be created soon

            // Register the Main Window itself, so it can be resolved with its DataContext
            // AddSingleton if you only want one instance of the main window throughout the app's lifetime.
            services.AddSingleton<MainWindow>();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Get the MainWindow instance from the ServiceProvider
            // Its DataContext (MainViewModel) will be injected automatically if its constructor requests it.
            var mainWindow = _serviceProvider.GetService<MainWindow>();

            // If MainWindow's constructor doesn't take MainViewModel as a parameter, you'd set it here:
            // mainWindow.DataContext = _serviceProvider.GetService<MainViewModel>();

            mainWindow.Show();
        }

        // Provide a static way to access the ServiceProvider from other parts of the application
        // This is useful for views/viewmodels that need to resolve other services or viewmodels dynamically.
        public static ServiceProvider ServiceProvider { get; private set; }

        protected override void OnInitialized()
        {
            base.OnInitialized();
            ServiceProvider = _serviceProvider; // Assign the service provider once built
        }
    }
}
