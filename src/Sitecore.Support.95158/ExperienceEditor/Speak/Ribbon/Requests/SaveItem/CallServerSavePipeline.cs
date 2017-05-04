namespace Sitecore.Support.ExperienceEditor.Speak.Ribbon.Requests.SaveItem
{
  using Server.Contexts;
  using Sitecore.Data;
  using Sitecore.ExperienceEditor.Speak.Server.Requests;
  using Sitecore.ExperienceEditor.Speak.Server.Responses;
  using Sitecore.ExperienceEditor.Switchers;
  using Sitecore.Pipelines;
  using Sitecore.Pipelines.Save;

  public class CallServerSavePipeline : PipelineProcessorRequest<PageContextNoLayoutDelta>
    {
        public override PipelineProcessorResponseValue ProcessRequest()
        {
            PipelineProcessorResponseValue value2 = new PipelineProcessorResponseValue();
            Pipeline pipeline = PipelineFactory.GetPipeline("saveUI");
            pipeline.ID = ShortID.Encode(ID.NewID);
            SaveArgs saveArgs = base.RequestContext.GetSaveArgs();
            using (new ClientDatabaseSwitcher(base.RequestContext.Item.Database))
            {
                pipeline.Start(saveArgs);
                value2.AbortMessage = saveArgs.Error;
                return value2;
            }
        }
    }
}

