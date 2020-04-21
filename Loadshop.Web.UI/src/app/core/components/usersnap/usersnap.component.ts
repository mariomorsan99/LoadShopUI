import { Component, Input, OnChanges, SimpleChanges, ChangeDetectionStrategy } from '@angular/core';
import { User } from 'src/app/shared/models';
import { environment } from 'src/environments/environment';

@Component({
  selector: 'kbxl-usersnap',
  templateUrl: './usersnap.component.html',
  styleUrls: ['./usersnap.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class UsersnapComponent implements OnChanges {
  @Input() user: User;
  usersnapLoaded = false;

  ngOnChanges(changes: SimpleChanges): void {
    if (changes && changes.user && changes.user.currentValue) {
      this.usersnapInit();
    }
  }

  private usersnapInit() {
    if (environment.enableUsersnap && !this.usersnapLoaded && this.user) {
      document.getElementById('feedback-button-container').style.display = 'block';

      const usersnapRouterConfig = this.getUsersnapRouterConfig();
      this.usersnapClassicInit(this.user, usersnapRouterConfig);
      this.usersnapRouterInit(usersnapRouterConfig);
      this.usersnapFeedbackInit(this.user);
      this.usersnapLoaded = true;
    }
  }

  private getUsersnapRouterConfig() {
    return {
      CXApiKey: environment.usersnapCXApiKey, // Project [Routing Widget]
      ClassicApiKey: environment.usersnapClassicApiKey,
      title: 'Service Desk',
      button: false,
      header: true,
      buttons: [
        {
          title: 'Feedback',
          text: 'How are we doing?',
          textColor: '#f0f0f5',
          bgColor: '#23425a',
          icon: 'feedbackIcon',
          action: {
            type: 'javascript',
            content: 'feedbackAPI.open()',
          },
        },
        {
          title: 'Bug Report',
          text: 'Show us where the issue is.',
          textColor: '#f0f0f5',
          bgColor: '#23425a',
          icon: 'bugIcon',
          action: {
            type: 'javascript',
            content: 'UsersnapClassic.open()',
          },
        },
      ],
    };
  }

  private usersnapClassicInit(user: User, usersnapRouterConfig) {
    const emailAddress = this.getEmailAddress(user);

    window['onUsersnapLoadClassic'] = function(api) {
      api.init({
        button: null,
      });

      api.on('open', function(event) {
        event.api.setValue('email', emailAddress);
        event.api.setValue(
          'customData',
          JSON.stringify({
            userId: user.userId,
            name: user.name,
            entityId: user.focusEntity.id,
            entityName: user.focusEntity.name,
            entityType: user.focusEntity.type,
          })
        );
      });

      window['UsersnapClassic'] = api;
    };
    const userSnapApiScript = document.createElement('script');
    userSnapApiScript.async = true;
    userSnapApiScript.src = 'https://api.usersnap.com/load/' + usersnapRouterConfig.ClassicApiKey + '.js?onload=onUsersnapLoadClassic';
    document.getElementsByTagName('head')[0].appendChild(userSnapApiScript);
  }

  private usersnapRouterInit(usersnapRouterConfig) {
    window['onUsersnapCXLoad'] = function(api) {
      window['UsersnapFeedbackRouter'](api, usersnapRouterConfig);

      document.getElementById('feedback-button').onclick = function collectFeedback() {
        api.open();
      };

      window['usersnapSetHeaderStyle'] = function(attempt) {
        const iframe = document.getElementsByName('us-entrypoint-widgetApp')[0];
        let cntnt = iframe['contentWindow'] || iframe['contentDocument'];
        if (cntnt.document) {
          cntnt = cntnt.document;
        }
        const lh = cntnt.getElementById('logo_header.png');
        if (lh) {
          lh.style.backgroundColor = '#23aae6';
        } else {
          if (attempt < 3) {
            attempt++;
            setTimeout(function() {
              window['usersnapSetHeaderStyle'](attempt);
            }, attempt * 100);
          }
        }
      };

      api.on('open', function(event) {
        setTimeout(function() {
          window['usersnapSetHeaderStyle'](1);
        }, 100);
      });
    };
    const userSnapBugScript = document.createElement('script');
    userSnapBugScript.async = true;
    userSnapBugScript.src = 'https://widget.usersnap.com/load/' + usersnapRouterConfig.CXApiKey + '?onload=onUsersnapCXLoad&isdev=true';
    document.getElementsByTagName('head')[0].appendChild(userSnapBugScript);
  }

  private usersnapFeedbackInit(user: User) {
    const emailAddress = this.getEmailAddress(user);

    window['onUsersnapFeedbackLoad'] = function(api) {
      api.init({
        button: null,
      });

      api.on('open', function(event) {
        event.api.setValue('visitor', emailAddress);
        event.api.setValue('custom', {
          userId: user.userId,
          name: user.name,
          entityId: user.focusEntity.id,
          entityName: user.focusEntity.name,
          entityType: user.focusEntity.type,
        });
      });

      window['feedbackAPI'] = api;
    };
    const userSnapFeedbackScript = document.createElement('script');
    userSnapFeedbackScript.async = true;
    userSnapFeedbackScript.src =
      'https://widget.usersnap.com/load/' + environment.usersnapFeedbackApiKey + '?onload=onUsersnapFeedbackLoad';
    document.getElementsByTagName('head')[0].appendChild(userSnapFeedbackScript);
  }

  private getEmailAddress(user: User) {
    if (user && user.userNotifications) {
      const emailNotification = user.userNotifications.find(function(element) {
        return element.messageTypeId === 'Email';
      });
      if (emailNotification) {
        return emailNotification.notificationValue;
      }
    }

    return null;
  }
}
