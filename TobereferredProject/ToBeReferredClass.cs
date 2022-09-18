namespace TobereferredProject
{
    public class TobereferredClass
    {
        public int Plus(int a, int b)
        {
            NotToBeReferredClass cl = new NotToBeReferredClass();
            return a + b;
        }
    }
}