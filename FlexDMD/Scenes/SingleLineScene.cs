﻿/* Copyright 2019 Vincent Bousquet

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
   */

namespace FlexDMD.Scenes
{
    class SingleLineScene : BackgroundScene
    {
        private readonly Label _text;
        private readonly bool _scroll;

        public float ScrollX { get; set; } = 0f;

        public SingleLineScene(IFlexDMD flex, Actor background, string text, Font font, AnimationType animateIn, float pauseS, AnimationType animateOut, bool scroll = false, string id = "") : base(flex, background, animateIn, pauseS, animateOut, id)
        {
            _scroll = scroll;
            _text = new Label(flex, font, text);
            AddActor(_text);

        }

        public void SetText(string text)
        {
            _text.Text = text;
        }

        protected override void Begin()
        {
            base.Begin();
            if (_scroll)
            {
                // UltraDMD moves text by 1.2 pixel per frame (no delta time) and seems to render based on a frame rate at 60FPS.
                var offset = -(Width - _text.Width) / 2f;
                ScrollX = offset + Width;
                _tweener.Tween(this, new { ScrollX = offset - _text.Width }, (Width + _text.Width) / (60 * 1.2f), 0f);
            }
        }

        public override void Update(float delta)
        {
            base.Update(delta);
            _text.SetPosition((Width - _text.Width) / 2 + ScrollX, (Height - _text.Height) / 2);
        }
    }
}
