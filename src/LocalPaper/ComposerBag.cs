namespace LocalPaper;

using System;
using System.ComponentModel;
using System.Drawing;

[DisplayName("{Section,nq}")]
internal record ComposerBag(string Section, IComposer Composer, Rectangle Rectangle, bool IsInverted, TimeSpan Offset);
