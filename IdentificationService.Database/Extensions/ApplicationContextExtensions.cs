using IdentificationService.Database.DataContext;
using IdentificationService.Database.Interfaces;
using System.Data;
using System.Reflection;

namespace IdentificationService.Database.Extensions
{
    internal static class ApplicationContextExtensions
    {
        public static void SeedData(this ApplicationContext context)
        {
            IOrderedEnumerable<IDataSeeder> dataSeeders = GetDataSeeders();

            foreach (var dataSeeder in dataSeeders)
            {
                dataSeeder.Seed(context);
                context.SaveChanges();
            }
        }

        private static IOrderedEnumerable<IDataSeeder> GetDataSeeders()
        {
            return Assembly.GetExecutingAssembly()
                            .GetTypes()
                            .Where(o => o.IsClass && o.IsAssignableTo(typeof(IDataSeeder)))
                            .Select(o => Activator.CreateInstance(o))
                            .OfType<IDataSeeder>()
                            .OrderBy(o => o.SequenceNumber);
        }
    }
}
