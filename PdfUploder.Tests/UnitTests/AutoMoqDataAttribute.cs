
using AutoFixture;
using AutoFixture.AutoMoq;
using AutoFixture.Xunit2;

namespace PdfUploder.Tests.UnitTests
{
    public sealed class AutoMoqDataAttribute : AutoDataAttribute
    {
        //public AutoMoqDataAttribute()
        //  : base(() =>
        //  {
        //      var fixture = new Fixture();
        //      fixture.Customize(new AutoMoqCustomization() { ConfigureMembers = true });              
        //      return fixture;
        //  })
        //{

        //}


        public AutoMoqDataAttribute()
            : base(() => new Fixture().Customize(new AutoMoqCustomization() { ConfigureMembers = true }))
        {

        }

        //public AutoMoqDataAttribute() : base(() =>
        //{
        //    var fixture = new Fixture().Customize(new CompositeCustomization(
        //        new AutoMoqCustomization() { ConfigureMembers = true },
        //        new SupportMutableValueTypesCustomization()));

        //    fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList().ForEach(b => fixture.Behaviors.Remove(b));
        //    fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        //    return fixture;
        //})
        //{
        //}
    }
}
