﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Chroma.NetCore.Api.Chroma;
using Chroma.NetCore.Api.Extensions;
using Chroma.NetCore.Api.Messages;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Chroma.NetCore.Api.Interfaces
{
    public abstract class DeviceBase : IDevice, IDeviceData
    {

        private IClient client;

        #region Properties

        public abstract string Device { get; }
        public virtual List<Effect> Supports { get; }
        public Effect ActiveEffect { get; private set; }
        public string EffectId { get; }

        #endregion

        protected DeviceBase(IClient client)
        {
            this.client = client;
            ActiveEffect = Effect.Undefined;
            EffectData = null;
        }

        public dynamic EffectData { get; set; }

        public async Task SetStatic(Color color)
        {
            await SetDeviceEffect(Effect.ChromaStatic, color);
        }

        public async Task SetAll(Color color)
        {
           await  SetStatic(color);
        }

        public async Task SetNone()
        {
           await SetDeviceEffect(Effect.ChromaNone);
        }

        protected async Task<string> SetDeviceEffect(Effect effect, dynamic data = null)
        {
            this.ActiveEffect = effect;
            this.EffectData = GenerateMessage(data);
            
            var headsetMessage = new DeviceMessage(this);
            var response = await client.Request(headsetMessage);

            return response;
        }


        private class Data
        {
            public string Effect { get; set; }
            public dynamic Param { get; set; }
        }

        internal dynamic GenerateMessage(dynamic data)
        {
            var message = new Data();

            switch (ActiveEffect)
            {

                case Effect.ChromaNone:
                    message.Effect = ActiveEffect.GetStringValue();
                    message.Param = null;
                    break;

                case Effect.ChromaStatic:
                    message.Effect = ActiveEffect.GetStringValue();
                    message.Param = new { color = ((Color)data).ToBgr() };
                    break;

                case Effect.ChromaCustom2:
                case Effect.ChromaCustom:
                    message.Effect = ActiveEffect.GetStringValue();
                    message.Param = data;
                    break;

                case Effect.ChromaCustomKey:
                    message.Effect = ActiveEffect.GetStringValue();
                    message.Param = data;
                    break;
            }

            var jsonString = JsonConvert.SerializeObject(message, Formatting.Indented,
                new JsonSerializerSettings() {ContractResolver = new CamelCasePropertyNamesContractResolver()});

            return jsonString;
        }

    }
}
