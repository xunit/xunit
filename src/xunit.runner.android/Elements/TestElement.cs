//
// Copyright 2011-2012 Xamarin Inc.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//   http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//

using System;
using Android.Graphics;

namespace Xunit.Runners.UI
{

    internal abstract class TestElement : FormattedElement
    {
        
        protected TestElement(AndroidRunner runner)
            : base(String.Empty)
        {
            if (runner == null) throw new ArgumentNullException("runner");

            Runner = runner;
        }

     

        protected virtual void OptionsChanged()
        {

        }

        public abstract TestState Result { get; }

        protected abstract string GetCaption();

        public void Refresh()
        {
            var caption = GetCaption();
            SetCaption(caption);
        }

        protected AndroidRunner Runner { get; private set; }
    }
}