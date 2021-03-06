﻿// ******************************************************************
// Copyright (c) Microsoft. All rights reserved.
// This code is licensed under the MIT License (MIT).
// THE CODE IS PROVIDED “AS IS”, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
// INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
// IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM,
// DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
// TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH
// THE CODE OR THE USE OR OTHER DEALINGS IN THE CODE.
// ******************************************************************

namespace Microsoft.Templates.UI.ViewModels.Common
{
    public enum StatusType
    {
        Empty,
        Information,
        Warning,
        Error
    }

    public class StatusViewModel
    {
        public static StatusViewModel EmptyStatus = new StatusViewModel(StatusType.Empty);

        public StatusType Status { get; set; }
        public string Message { get; set; }
        public int AutoHideSeconds { get; set; }

        public StatusViewModel(StatusType status, string message = null, int autoHide = 0)
        {
            AutoHideSeconds = autoHide;
            Message = message;
            Status = status;
        }

        public static StatusViewModel Information(string message, int autoHide = 0) => new StatusViewModel(StatusType.Information, message, autoHide);
        public static StatusViewModel Warning(string message, int autoHide = 0) => new StatusViewModel(StatusType.Warning, message, autoHide);
        public static StatusViewModel Error(string message, int autoHide = 0) => new StatusViewModel(StatusType.Error, message, autoHide);
    }
}
